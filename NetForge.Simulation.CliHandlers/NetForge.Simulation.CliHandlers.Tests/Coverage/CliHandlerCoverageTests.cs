using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.CliHandlers.Anira;
using NetForge.Simulation.CliHandlers.Aruba;
using NetForge.Simulation.CliHandlers.Cisco;
using NetForge.Simulation.CliHandlers.Linux;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Coverage
{
    public class CliHandlerCoverageTests
    {
        public static IEnumerable<object[]> HandlerTypes()
        {
            var map = new Dictionary<string, Assembly>
            {
                { "Anira", typeof(AniraVendorContext).Assembly },
                { "Aruba", typeof(ArubaVendorContext).Assembly },
                { "Cisco", typeof(CiscoVendorContext).Assembly },
                { "Linux", typeof(LinuxVendorContext).Assembly },
            };

            foreach (var (vendor, asm) in map)
            {
                foreach (var t in asm.GetTypes())
                {
                    if (typeof(VendorAgnosticCliHandler).IsAssignableFrom(t) && !t.IsAbstract)
                    {
                        yield return new object[] { t, vendor };
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(HandlerTypes))]
        public void AllHandlersShouldInstantiateAndHandleCommand(Type handlerType, string vendor)
        {
            // Arrange
            var handler = (VendorAgnosticCliHandler)Activator.CreateInstance(handlerType)!;
            var command = handler.GetCommandInfo()?.Item1 ?? string.Empty;
            NetworkDevice device = vendor switch
            {
                "Anira" => new AniraDevice("test"),
                "Aruba" => new ArubaDevice("test"),
                "Cisco" => new CiscoDevice("test"),
                "Linux" => new LinuxDevice("test"),
                _ => throw new ArgumentException("unknown vendor")
            };
            device.SetMode("privileged");
            var context = new CliContext(device, new[] { command }, command)
            {
                VendorContext = vendor switch
                {
                    "Anira" => new AniraVendorContext(device),
                    "Aruba" => new ArubaVendorContext(device),
                    "Cisco" => new CiscoVendorContext(device),
                    "Linux" => new LinuxVendorContext(device),
                    _ => null
                }
            };

            // Act
            var result = handler.Handle(context);

            // Assert
            Assert.NotNull(result);
        }
    }
}
