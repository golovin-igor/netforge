using System;
using System.Collections.Generic;
using System.Linq;
using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.CliHandlers.Cisco;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Cisco
{
    public class CiscoHandlersCoverageTests
    {
        public static IEnumerable<object[]> HandlerTypes()
        {
            var assembly = typeof(CiscoHandlerRegistry).Assembly;
            return assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(ICliHandler).IsAssignableFrom(t) && t.Name.EndsWith("Handler"))
                .Select(t => new object[] { t });
        }

        [Theory]
        [MemberData(nameof(HandlerTypes))]
        public void HandlerShouldExecuteWithoutThrowing(Type handlerType)
        {
            // Arrange
            var registry = new CiscoHandlerRegistry();
            registry.Initialize();
            var device = new CiscoDevice("TestDevice");
            var handler = (ICliHandler)Activator.CreateInstance(handlerType)!;
            var info = (ValueTuple<string, string>?)handler.GetType().GetMethod("GetCommandInfo")!.Invoke(handler, null);
            var command = info?.Item1 ?? string.Empty;
            var context = new CliContext(device, new[] { command }, command);

            // Act
            var result = handler.Handle(context);

            // Assert
            Assert.NotNull(result);
        }
    }
}
