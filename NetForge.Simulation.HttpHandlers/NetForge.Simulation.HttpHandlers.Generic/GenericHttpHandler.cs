using NetForge.Simulation.HttpHandlers.Common;

namespace NetForge.Simulation.HttpHandlers.Generic
{
    /// <summary>
    /// Generic HTTP handler for standard features across all vendors
    /// </summary>
    public class GenericHttpHandler : BaseHttpHandler
    {
        public override string VendorName => "Generic";
        public override int Priority => 50; // Lower priority than vendor-specific handlers

        public GenericHttpHandler(
            IHttpAuthenticator authenticator,
            IHttpApiProvider apiProvider,
            IHttpContentProvider contentProvider)
            : base(authenticator, apiProvider, contentProvider)
        {
        }

        public override async Task<string> GenerateWebInterface(HttpContext context)
        {
            var deviceInfo = await GetDeviceInfo();
            var interfaceStatus = await GetInterfaceStatus();

            return GenerateGenericWebPage(deviceInfo, interfaceStatus);
        }

        private string GenerateGenericWebPage(DeviceInfo device, List<InterfaceInfo> interfaces)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <title>{device.Vendor} {device.Model} - {device.Hostname}</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .header {{
            background-color: #333;
            color: white;
            padding: 20px;
            border-radius: 5px;
            margin-bottom: 20px;
        }}
        .device-info {{
            background-color: white;
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .interface-table {{
            width: 100%;
            border-collapse: collapse;
            background-color: white;
            border-radius: 5px;
            overflow: hidden;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .interface-table th, .interface-table td {{
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #ddd;
        }}
        .interface-table th {{
            background-color: #f8f9fa;
            font-weight: bold;
        }}
        .status-up {{
            color: #28a745;
            font-weight: bold;
        }}
        .status-down {{
            color: #dc3545;
            font-weight: bold;
        }}
        .stats {{
            display: flex;
            gap: 20px;
            margin-bottom: 20px;
        }}
        .stat {{
            background-color: white;
            padding: 15px;
            border-radius: 5px;
            text-align: center;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .stat-value {{
            font-size: 24px;
            font-weight: bold;
            color: #333;
        }}
        .stat-label {{
            color: #666;
            margin-top: 5px;
        }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>{device.Vendor} {device.Model}</h1>
        <p>Device Management Interface</p>
    </div>

    <div class=""device-info"">
        <h3>Device Information</h3>
        <p><strong>Hostname:</strong> {device.Hostname}</p>
        <p><strong>Software Version:</strong> {device.SoftwareVersion}</p>
        <p><strong>Uptime:</strong> {device.Uptime}</p>
        <p><strong>CPU Usage:</strong> {device.CpuUsage}%</p>
        <p><strong>Memory Usage:</strong> {device.MemoryUsage}%</p>
    </div>

    <div class=""stats"">
        <div class=""stat"">
            <div class=""stat-value"">{interfaces.Count(i => i.IsUp)}</div>
            <div class=""stat-label"">Interfaces Up</div>
        </div>
        <div class=""stat"">
            <div class=""stat-value"">{interfaces.Count(i => !i.IsUp)}</div>
            <div class=""stat-label"">Interfaces Down</div>
        </div>
        <div class=""stat"">
            <div class=""stat-value"">{interfaces.Count}</div>
            <div class=""stat-label"">Total Interfaces</div>
        </div>
    </div>

    <h3>Interface Status</h3>
    <table class=""interface-table"">
        <thead>
            <tr>
                <th>Interface</th>
                <th>Status</th>
                <th>IP Address</th>
                <th>Description</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", interfaces.Select(i => $@"
            <tr>
                <td>{i.Name}</td>
                <td><span class=""status-{(i.IsUp ? "up" : "down")}"">{(i.IsUp ? "Up" : "Down")}</span></td>
                <td>{i.IpAddress}</td>
                <td>{i.Description}</td>
            </tr>"))}
        </tbody>
    </table>

    <div style=""margin-top: 30px; padding: 15px; background-color: white; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
        <h3>Available Actions</h3>
        <ul>
            <li><a href=""/api/system/info"">System Information API</a></li>
            <li><a href=""/api/interfaces"">Interface Management API</a></li>
            <li><a href=""/api/config/running"">Running Configuration</a></li>
            <li><a href=""/api/protocols"">Protocol Status</a></li>
        </ul>
    </div>
</body>
</html>";
        }

        protected override async Task<HttpResult> OnHandlePostRequest(HttpContext context)
        {
            var path = context.Request.Path.ToLower();

            return path switch
            {
                "/api/system/restart" => await RestartSystem(context),
                "/api/system/info" => await GetSystemInfo(context),
                "/api/interfaces" => await GetInterfaces(context),
                _ => HttpResult.NotFound("Endpoint not found")
            };
        }

        private async Task<HttpResult> RestartSystem(HttpContext context)
        {
            try
            {
                // Simulate system restart
                await Task.Delay(1000);
                return HttpResult.Ok(new { message = "System restart initiated", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return HttpResult.Error(500, $"Restart failed: {ex.Message}");
            }
        }

        private async Task<HttpResult> GetSystemInfo(HttpContext context)
        {
            var deviceInfo = await GetDeviceInfo();
            return HttpResult.Ok(deviceInfo);
        }

        private async Task<HttpResult> GetInterfaces(HttpContext context)
        {
            var interfaces = await GetInterfaceStatus();
            return HttpResult.Ok(interfaces);
        }

        public override IEnumerable<HttpEndpoint> GetSupportedEndpoints()
        {
            return new[]
            {
                new HttpEndpoint { Path = "/", Method = "GET", Description = "Generic Dashboard" },
                new HttpEndpoint { Path = "/api/system/info", Method = "GET", Description = "Get System Information" },
                new HttpEndpoint { Path = "/api/system/restart", Method = "POST", Description = "Restart System" },
                new HttpEndpoint { Path = "/api/interfaces", Method = "GET", Description = "Get Interface Status" },
                new HttpEndpoint { Path = "/api/config/running", Method = "GET", Description = "Get Running Configuration" },
                new HttpEndpoint { Path = "/api/protocols", Method = "GET", Description = "Get Protocol Status" }
            };
        }

        private async Task<DeviceInfo> GetDeviceInfo()
        {
            return new DeviceInfo
            {
                Vendor = _device.Vendor,
                Hostname = _device.Hostname ?? _device.Name,
                Model = _device.DeviceType,
                SoftwareVersion = "1.0.0", // Generic version
                Uptime = GetUptime(),
                CpuUsage = GetCpuUsage(),
                MemoryUsage = GetMemoryUsage()
            };
        }

        private async Task<List<InterfaceInfo>> GetInterfaceStatus()
        {
            var interfaces = new List<InterfaceInfo>();

            foreach (var kvp in _device.GetAllInterfaces())
            {
                var config = _device.GetInterface(kvp.Key);
                if (config != null)
                {
                    interfaces.Add(new InterfaceInfo
                    {
                        Name = kvp.Key,
                        IsUp = config.IsUp && !config.IsShutdown,
                        IpAddress = config.IpAddress ?? "Not configured",
                        SubnetMask = config.SubnetMask ?? "",
                        Description = config.Description ?? ""
                    });
                }
            }

            return interfaces;
        }

        private string GetUptime()
        {
            var random = new Random(_device.Name.GetHashCode());
            var days = random.Next(1, 30);
            var hours = random.Next(0, 24);
            return $"{days} days, {hours} hours";
        }

        private int GetCpuUsage()
        {
            var random = new Random(_device.Name.GetHashCode() + DateTime.Now.Hour);
            return random.Next(10, 40);
        }

        private int GetMemoryUsage()
        {
            var random = new Random(_device.Name.GetHashCode() + DateTime.Now.Minute);
            return random.Next(20, 60);
        }
    }

    // Supporting classes
    public class DeviceInfo
    {
        public string Vendor { get; set; } = "";
        public string Hostname { get; set; } = "";
        public string Model { get; set; } = "";
        public string SoftwareVersion { get; set; } = "";
        public string Uptime { get; set; } = "";
        public int CpuUsage { get; set; }
        public int MemoryUsage { get; set; }
    }

    public class InterfaceInfo
    {
        public string Name { get; set; } = "";
        public bool IsUp { get; set; }
        public string IpAddress { get; set; } = "";
        public string SubnetMask { get; set; } = "";
        public string Description { get; set; } = "";
    }
}