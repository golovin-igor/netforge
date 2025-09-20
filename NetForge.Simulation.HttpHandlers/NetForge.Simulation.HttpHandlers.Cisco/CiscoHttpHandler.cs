using NetForge.Simulation.HttpHandlers.Common;

namespace NetForge.Simulation.HttpHandlers.Cisco
{
    /// <summary>
    /// Cisco-specific HTTP handler with IOS-style web interface
    /// </summary>
    public class CiscoHttpHandler : BaseHttpHandler
    {
        public override string VendorName => "Cisco";
        public override int Priority => 200; // Higher priority for vendor-specific

        public CiscoHttpHandler(
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
            var routingTable = await GetRoutingTable();

            return GenerateCiscoWebPage(deviceInfo, interfaceStatus, routingTable);
        }

        private string GenerateCiscoWebPage(DeviceInfo device, List<InterfaceInfo> interfaces, List<RouteInfo> routes)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <title>Cisco {device.Model} - {device.Hostname}</title>
    <link rel=""stylesheet"" href=""/static/cisco/styles.css"">
    <script src=""/static/cisco/scripts.js""></script>
</head>
<body>
    <div class=""cisco-header"">
        <img src=""/static/cisco/logo.png"" alt=""Cisco"" class=""logo"">
        <h1>{device.Hostname}</h1>
        <div class=""device-info"">
            <span>Model: {device.Model}</span>
            <span>IOS Version: {device.SoftwareVersion}</span>
            <span>Uptime: {device.Uptime}</span>
        </div>
    </div>

    <div class=""navigation"">
        <ul>
            <li><a href=""/"" class=""active"">Dashboard</a></li>
            <li><a href=""/interfaces"">Interfaces</a></li>
            <li><a href=""/routing"">Routing</a></li>
            <li><a href=""/protocols"">Protocols</a></li>
            <li><a href=""/security"">Security</a></li>
            <li><a href=""/monitoring"">Monitoring</a></li>
            <li><a href=""/configuration"">Configuration</a></li>
        </ul>
    </div>

    <div class=""content"">
        <div class=""dashboard-widgets"">
            <div class=""widget interface-summary"">
                <h3>Interface Summary</h3>
                <div class=""interface-stats"">
                    <div class=""stat"">
                        <span class=""value"">{interfaces.Count(i => i.IsUp)}</span>
                        <span class=""label"">Interfaces Up</span>
                    </div>
                    <div class=""stat"">
                        <span class=""value"">{interfaces.Count(i => !i.IsUp)}</span>
                        <span class=""label"">Interfaces Down</span>
                    </div>
                </div>
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
                        {string.Join("", interfaces.Take(5).Select(i => $@"
                        <tr class=""{(i.IsUp ? "up" : "down")}"">
                            <td>{i.Name}</td>
                            <td><span class=""status {(i.IsUp ? "up" : "down")}"">{(i.IsUp ? "Up" : "Down")}</span></td>
                            <td>{i.IpAddress}</td>
                            <td>{i.Description}</td>
                        </tr>"))}
                    </tbody>
                </table>
            </div>

            <div class=""widget routing-summary"">
                <h3>Routing Summary</h3>
                <div class=""routing-stats"">
                    <div class=""stat"">
                        <span class=""value"">{routes.Count}</span>
                        <span class=""label"">Total Routes</span>
                    </div>
                    <div class=""stat"">
                        <span class=""value"">{routes.Count(r => r.Protocol == "Connected")}</span>
                        <span class=""label"">Connected</span>
                    </div>
                    <div class=""stat"">
                        <span class=""value"">{routes.Count(r => r.Protocol == "OSPF")}</span>
                        <span class=""label"">OSPF</span>
                    </div>
                </div>
            </div>

            <div class=""widget system-resources"">
                <h3>System Resources</h3>
                <div class=""resource-bars"">
                    <div class=""resource"">
                        <label>CPU Usage</label>
                        <div class=""progress-bar"">
                            <div class=""progress"" style=""width: {device.CpuUsage}%""></div>
                        </div>
                        <span>{device.CpuUsage}%</span>
                    </div>
                    <div class=""resource"">
                        <label>Memory Usage</label>
                        <div class=""progress-bar"">
                            <div class=""progress"" style=""width: {device.MemoryUsage}%""></div>
                        </div>
                        <span>{device.MemoryUsage}%</span>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script>
        // Auto-refresh every 30 seconds
        setTimeout(function() {{ location.reload(); }}, 30000);
    </script>
</body>
</html>";
        }

        protected override async Task<HttpResult> OnHandlePostRequest(HttpContext context)
        {
            // Handle Cisco-specific POST requests
            var path = context.Request.Path.ToLower();

            return path switch
            {
                "/api/interfaces/configure" => await ConfigureInterface(context),
                "/api/routing/add" => await AddRoute(context),
                "/api/protocols/configure" => await ConfigureProtocol(context),
                "/api/save-config" => await SaveConfiguration(context),
                _ => HttpResult.NotFound("Endpoint not found")
            };
        }

        private async Task<HttpResult> ConfigureInterface(HttpContext context)
        {
            var request = context.Request.GetBodyAsJson<InterfaceConfigRequest>();
            if (request == null)
            {
                return HttpResult.BadRequest("Invalid request format");
            }

            try
            {
                // Apply interface configuration using CLI
                var commands = new List<string>
                {
                    "configure terminal",
                    $"interface {request.InterfaceName}",
                    request.IsEnabled ? "no shutdown" : "shutdown",
                    $"ip address {request.IpAddress} {request.SubnetMask}",
                    $"description {request.Description}",
                    "exit",
                    "exit"
                };

                var results = new List<string>();
                foreach (var command in commands)
                {
                    var result = await _device.ProcessCommandAsync(command);
                    results.Add(result);
                }

                return HttpResult.Ok(new { success = true, results });
            }
            catch (Exception ex)
            {
                return HttpResult.Error(500, $"Configuration failed: {ex.Message}");
            }
        }

        private async Task<HttpResult> AddRoute(HttpContext context)
        {
            var request = context.Request.GetBodyAsJson<RouteConfigRequest>();
            if (request == null)
            {
                return HttpResult.BadRequest("Invalid request format");
            }

            try
            {
                var commands = new List<string>
                {
                    "configure terminal",
                    $"ip route {request.Destination} {request.SubnetMask} {request.NextHop}",
                    "exit"
                };

                var results = new List<string>();
                foreach (var command in commands)
                {
                    var result = await _device.ProcessCommandAsync(command);
                    results.Add(result);
                }

                return HttpResult.Ok(new { success = true, results });
            }
            catch (Exception ex)
            {
                return HttpResult.Error(500, $"Route configuration failed: {ex.Message}");
            }
        }

        private async Task<HttpResult> ConfigureProtocol(HttpContext context)
        {
            var request = context.Request.GetBodyAsJson<ProtocolConfigRequest>();
            if (request == null)
            {
                return HttpResult.BadRequest("Invalid request format");
            }

            try
            {
                var commands = new List<string>
                {
                    "configure terminal",
                    $"router {request.Protocol.ToLower()} {request.ProcessId}",
                    $"network {request.Network}",
                    "exit",
                    "exit"
                };

                var results = new List<string>();
                foreach (var command in commands)
                {
                    var result = await _device.ProcessCommandAsync(command);
                    results.Add(result);
                }

                return HttpResult.Ok(new { success = true, results });
            }
            catch (Exception ex)
            {
                return HttpResult.Error(500, $"Protocol configuration failed: {ex.Message}");
            }
        }

        private async Task<HttpResult> SaveConfiguration(HttpContext context)
        {
            try
            {
                var commands = new List<string>
                {
                    "copy running-config startup-config"
                };

                var results = new List<string>();
                foreach (var command in commands)
                {
                    var result = await _device.ProcessCommandAsync(command);
                    results.Add(result);
                }

                return HttpResult.Ok(new { success = true, results });
            }
            catch (Exception ex)
            {
                return HttpResult.Error(500, $"Save configuration failed: {ex.Message}");
            }
        }

        public override IEnumerable<HttpEndpoint> GetSupportedEndpoints()
        {
            return new[]
            {
                new HttpEndpoint { Path = "/", Method = "GET", Description = "Dashboard" },
                new HttpEndpoint { Path = "/interfaces", Method = "GET", Description = "Interface Management" },
                new HttpEndpoint { Path = "/routing", Method = "GET", Description = "Routing Table" },
                new HttpEndpoint { Path = "/protocols", Method = "GET", Description = "Protocol Status" },
                new HttpEndpoint { Path = "/security", Method = "GET", Description = "Security Configuration" },
                new HttpEndpoint { Path = "/monitoring", Method = "GET", Description = "System Monitoring" },
                new HttpEndpoint { Path = "/configuration", Method = "GET", Description = "Device Configuration" },
                new HttpEndpoint { Path = "/api/interfaces/configure", Method = "POST", Description = "Configure Interface" },
                new HttpEndpoint { Path = "/api/routing/add", Method = "POST", Description = "Add Static Route" },
                new HttpEndpoint { Path = "/api/protocols/configure", Method = "POST", Description = "Configure Protocol" },
                new HttpEndpoint { Path = "/api/save-config", Method = "POST", Description = "Save Configuration" }
            };
        }

        private async Task<DeviceInfo> GetDeviceInfo()
        {
            return new DeviceInfo
            {
                Hostname = _device.GetHostname() ?? _device.Name,
                Model = _device.DeviceType,
                SoftwareVersion = "15.1(4)M12a", // Simulated IOS version
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

        private async Task<List<RouteInfo>> GetRoutingTable()
        {
            var routes = new List<RouteInfo>();

            // Simulate routing table entries
            var random = new Random(_device.Name.GetHashCode());
            var routeCount = random.Next(5, 15);

            for (int i = 0; i < routeCount; i++)
            {
                routes.Add(new RouteInfo
                {
                    Destination = $"192.168.{random.Next(1, 255)}.0",
                    SubnetMask = "255.255.255.0",
                    NextHop = $"192.168.{random.Next(1, 255)}.1",
                    Protocol = random.Next(3) == 0 ? "OSPF" : "Connected",
                    Metric = random.Next(1, 100),
                    Interface = $"GigabitEthernet{random.Next(0, 4)}/{random.Next(0, 48)}"
                });
            }

            return routes;
        }

        private string GetUptime()
        {
            // Simulate uptime calculation
            var random = new Random(_device.Name.GetHashCode());
            var days = random.Next(1, 100);
            var hours = random.Next(0, 24);
            var minutes = random.Next(0, 60);
            return $"{days} days, {hours} hours, {minutes} minutes";
        }

        private int GetCpuUsage()
        {
            // Simulate CPU usage
            var random = new Random(_device.Name.GetHashCode() + DateTime.Now.Hour);
            return random.Next(5, 25);
        }

        private int GetMemoryUsage()
        {
            // Simulate memory usage
            var random = new Random(_device.Name.GetHashCode() + DateTime.Now.Minute);
            return random.Next(30, 70);
        }
    }

    // Supporting classes
    public class DeviceInfo
    {
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

    public class RouteInfo
    {
        public string Destination { get; set; } = "";
        public string SubnetMask { get; set; } = "";
        public string NextHop { get; set; } = "";
        public string Protocol { get; set; } = "";
        public int Metric { get; set; }
        public string Interface { get; set; } = "";
    }

    public class InterfaceConfigRequest
    {
        public string InterfaceName { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public string SubnetMask { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
    }

    public class RouteConfigRequest
    {
        public string Destination { get; set; } = "";
        public string SubnetMask { get; set; } = "";
        public string NextHop { get; set; } = "";
    }

    public class ProtocolConfigRequest
    {
        public string Protocol { get; set; } = "";
        public string ProcessId { get; set; } = "";
        public string Network { get; set; } = "";
    }
}