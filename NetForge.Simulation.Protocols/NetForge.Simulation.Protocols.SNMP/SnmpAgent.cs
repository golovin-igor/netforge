using System.Net;
using System.Net.Sockets;
using System.Text;
using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.Protocols.SNMP;

public class SnmpAgent(INetworkDevice device, SnmpConfig config, SnmpState state) : IDisposable
{
    private readonly INetworkDevice _device = device;
    private UdpClient? _udpListener;
    private UdpClient? _trapSender;
    private Task? _listenerTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _disposed;

    public event EventHandler<SnmpRequestEventArgs>? RequestReceived;
    public event EventHandler<SnmpResponseEventArgs>? ResponseSent;

    public async Task StartAsync()
    {
        if (_disposed || state.AgentRunning)
            return;

        try
        {
            _udpListener = new UdpClient(config.Port);
            _trapSender = new UdpClient();

            _listenerTask = Task.Run(ListenForRequests, _cancellationTokenSource.Token);

            state.AgentRunning = true;
            state.StartTime = DateTime.Now;
            state.MarkStateChanged();

            _device.AddLogEntry($"SNMP agent started on port {config.Port}");
        }
        catch (Exception ex)
        {
            _device.AddLogEntry($"Failed to start SNMP agent: {ex.Message}");
            state.AgentRunning = false;
            throw;
        }
    }

    public async Task StopAsync()
    {
        if (_disposed || !state.AgentRunning)
            return;

        _cancellationTokenSource.Cancel();

        _udpListener?.Close();
        _trapSender?.Close();

        if (_listenerTask != null)
        {
            try
            {
                await _listenerTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling
            }
        }

        state.AgentRunning = false;
        state.MarkStateChanged();

        _device.AddLogEntry("SNMP agent stopped");
    }

    private async Task ListenForRequests()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested && _udpListener != null)
        {
            try
            {
                var result = await _udpListener.ReceiveAsync();
                _ = Task.Run(() => ProcessRequest(result.Buffer, result.RemoteEndPoint), _cancellationTokenSource.Token);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"SNMP listener error: {ex.Message}");
            }
        }
    }

    private async Task ProcessRequest(byte[] data, IPEndPoint remoteEndPoint)
    {
        try
        {
            var request = ParseSnmpRequest(data, remoteEndPoint.Address.ToString());
            if (request == null)
            {
                state.IncrementErrors();
                return;
            }

            state.IncrementRequests();
            state.UpdateActivity();

            // Validate community string
            if (config.EnableAuthentication && !IsValidCommunity(request.Community, request.RequestType))
            {
                _device.AddLogEntry($"SNMP authentication failed for community '{request.Community}' from {remoteEndPoint}");
                state.IncrementErrors();
                return;
            }

            var response = await ProcessSnmpRequest(request);

            if (response != null)
            {
                var responseData = EncodeSnmpResponse(response, request.Community);
                await _udpListener!.SendAsync(responseData, remoteEndPoint);
                state.IncrementResponses();

                ResponseSent?.Invoke(this, new SnmpResponseEventArgs(response, remoteEndPoint.ToString()));
            }

            RequestReceived?.Invoke(this, new SnmpRequestEventArgs(request, remoteEndPoint.ToString()));
        }
        catch (Exception ex)
        {
            _device.AddLogEntry($"Error processing SNMP request: {ex.Message}");
            state.IncrementErrors();
        }
    }

    private bool IsValidCommunity(string community, SnmpRequestType requestType)
    {
        return requestType switch
        {
            SnmpRequestType.Get or SnmpRequestType.GetNext or SnmpRequestType.GetBulk =>
                config.ReadCommunities.Contains(community),
            SnmpRequestType.Set =>
                config.WriteCommunities.Contains(community),
            _ => false
        };
    }

    private async Task<SnmpResponse?> ProcessSnmpRequest(SnmpRequest request)
    {
        var response = new SnmpResponse
        {
            RequestId = request.RequestId
        };

        try
        {
            switch (request.RequestType)
            {
                case SnmpRequestType.Get:
                    await ProcessGetRequest(request, response);
                    break;

                case SnmpRequestType.GetNext:
                    await ProcessGetNextRequest(request, response);
                    break;

                case SnmpRequestType.Set:
                    await ProcessSetRequest(request, response);
                    break;

                default:
                    response.ErrorStatus = SnmpErrorStatus.GenErr;
                    break;
            }
        }
        catch (Exception ex)
        {
            _device.AddLogEntry($"Error processing SNMP {request.RequestType}: {ex.Message}");
            response.ErrorStatus = SnmpErrorStatus.GenErr;
        }

        return response;
    }

    private async Task ProcessGetRequest(SnmpRequest request, SnmpResponse response)
    {
        if (state.MibDatabase.TryGetValue(request.Oid, out var variable))
        {
            response.VarBinds.Add(new SnmpVarBind
            {
                Oid = variable.Oid,
                Value = variable.Value,
                Type = variable.Type
            });
        }
        else
        {
            response.ErrorStatus = SnmpErrorStatus.NoSuchName;
            response.ErrorIndex = 1;
        }

        await Task.CompletedTask;
    }

    private async Task ProcessGetNextRequest(SnmpRequest request, SnmpResponse response)
    {
        var sortedOids = state.MibDatabase.Keys.OrderBy(k => k).ToList();
        var nextOid = sortedOids.FirstOrDefault(oid => String.Compare(oid, request.Oid, StringComparison.Ordinal) > 0);

        if (nextOid != null && state.MibDatabase.TryGetValue(nextOid, out var variable))
        {
            response.VarBinds.Add(new SnmpVarBind
            {
                Oid = variable.Oid,
                Value = variable.Value,
                Type = variable.Type
            });
        }
        else
        {
            response.ErrorStatus = SnmpErrorStatus.NoSuchName;
            response.ErrorIndex = 1;
        }

        await Task.CompletedTask;
    }

    private async Task ProcessSetRequest(SnmpRequest request, SnmpResponse response)
    {
        if (state.MibDatabase.TryGetValue(request.Oid, out var variable))
        {
            if (variable.IsReadOnly)
            {
                response.ErrorStatus = SnmpErrorStatus.ReadOnly;
                response.ErrorIndex = 1;
            }
            else
            {
                variable.Value = request.Value;
                variable.LastUpdated = DateTime.Now;

                response.VarBinds.Add(new SnmpVarBind
                {
                    Oid = variable.Oid,
                    Value = variable.Value,
                    Type = variable.Type
                });

                _device.AddLogEntry($"SNMP SET: {request.Oid} = {request.Value}");
                state.MarkStateChanged();
            }
        }
        else
        {
            response.ErrorStatus = SnmpErrorStatus.NoSuchName;
            response.ErrorIndex = 1;
        }

        await Task.CompletedTask;
    }

    public async Task SendTrapAsync(string trapOid, Dictionary<string, object> varbinds)
    {
        if (!config.EnableTraps || config.TrapDestinations.Count == 0)
            return;

        try
        {
            var trapData = EncodeTrap(trapOid, varbinds);

            foreach (var destination in config.TrapDestinations)
            {
                if (IPAddress.TryParse(destination, out var addr))
                {
                    var endpoint = new IPEndPoint(addr, config.TrapPort);
                    await _trapSender!.SendAsync(trapData, endpoint);
                    _device.AddLogEntry($"SNMP trap sent to {destination}: {trapOid}");
                }
            }

            state.IncrementTraps();
        }
        catch (Exception ex)
        {
            _device.AddLogEntry($"Error sending SNMP trap: {ex.Message}");
        }
    }

    private SnmpRequest? ParseSnmpRequest(byte[] data, string clientAddress)
    {
        // Simplified SNMP parsing - in reality would need full ASN.1/BER parsing
        try
        {
            var dataStr = Encoding.UTF8.GetString(data);

            // This is a simplified mock parser - real implementation would use proper SNMP libraries
            return new SnmpRequest
            {
                Community = "public", // Would parse from actual packet
                RequestType = SnmpRequestType.Get,
                Oid = "1.3.6.1.2.1.1.1.0", // Would parse from actual packet
                ClientAddress = clientAddress,
                RequestId = new Random().Next(1000, 9999)
            };
        }
        catch
        {
            return null;
        }
    }

    private byte[] EncodeSnmpResponse(SnmpResponse response, string community)
    {
        // Simplified SNMP encoding - real implementation would use proper ASN.1/BER encoding
        var responseStr = $"SNMP Response: RequestId={response.RequestId}, Status={response.ErrorStatus}";

        foreach (var varbind in response.VarBinds)
        {
            responseStr += $", {varbind.Oid}={varbind.Value}";
        }

        return Encoding.UTF8.GetBytes(responseStr);
    }

    private byte[] EncodeTrap(string trapOid, Dictionary<string, object> varbinds)
    {
        // Simplified trap encoding
        var trapStr = $"SNMP Trap: {trapOid}";
        foreach (var varbind in varbinds)
        {
            trapStr += $", {varbind.Key}={varbind.Value}";
        }
        return Encoding.UTF8.GetBytes(trapStr);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _cancellationTokenSource.Cancel();

        _udpListener?.Close();
        _trapSender?.Close();
        _udpListener?.Dispose();
        _trapSender?.Dispose();

        _cancellationTokenSource.Dispose();
        _disposed = true;
    }
}

public class SnmpRequestEventArgs(SnmpRequest request, string clientEndpoint) : EventArgs
{
    public SnmpRequest Request { get; } = request;
    public string ClientEndpoint { get; } = clientEndpoint;
}

public class SnmpResponseEventArgs(SnmpResponse response, string clientEndpoint) : EventArgs
{
    public SnmpResponse Response { get; } = response;
    public string ClientEndpoint { get; } = clientEndpoint;
}
