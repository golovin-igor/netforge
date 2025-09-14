using FluentAssertions;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Tests.TestHelpers;
using Xunit;

namespace NetForge.Tests.Services;

/// <summary>
/// Simple test to verify PingService is accessible
/// </summary>
public class PingServiceTests_Simple
{
    [Fact]
    public void PingService_CanBeInstantiated()
    {
        // Act
        var pingService = new PingService();

        // Assert
        pingService.Should().NotBeNull();
    }

    [Fact]
    public void MockDeviceBuilder_CanCreateDevice()
    {
        // Act
        var device = MockDeviceBuilder.Create()
            .WithName("TestDevice")
            .WithVendor("Cisco")
            .Build();

        // Assert
        device.Should().NotBeNull();
        device.Name.Should().Be("TestDevice");
        device.Vendor.Should().Be("Cisco");
    }
}