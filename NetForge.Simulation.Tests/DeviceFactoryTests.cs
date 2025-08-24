using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Core;
using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests
{
    public class DeviceFactoryTests
    {
        [Fact]
        public void CreateDevice_KnownVendor_ReturnsCorrectType()
        {
            var device = DeviceFactory.CreateDevice("cisco", "R1");
            Assert.IsType<CiscoDevice>(device);
        }

        [Fact]
        public void CreateDevice_LinuxVendor_ReturnsLinuxDevice()
        {
            var device = DeviceFactory.CreateDevice("linux", "host1");
            Assert.IsType<LinuxDevice>(device);
        }

        [Fact]
        public void CreateDevice_AlcatelVendor_ReturnsAlcatelDevice()
        {
            var device = DeviceFactory.CreateDevice("alcatel", "AL1");
            Assert.IsType<AlcatelDevice>(device);
        }

        [Fact]
        public void CreateDevice_F5Vendor_ReturnsF5Device()
        {
            var device = DeviceFactory.CreateDevice("f5", "F5-1");
            Assert.IsType<F5Device>(device);
        }

        [Fact]
        public void RegisterVendor_AllowsDynamicDeviceCreation()
        {
            DeviceFactory.RegisterVendor("testvendor", name => new DummyDevice(name));
            var device = DeviceFactory.CreateDevice("testvendor", "T1");
            Assert.IsType<DummyDevice>(device);
        }

        private class DummyDevice : NetworkDevice
        {
            public DummyDevice(string name) : base(name)
            {
                Vendor = "Dummy";
            }

            protected override void InitializeDefaultInterfaces()
            {
            }

            protected override void RegisterDeviceSpecificHandlers()
            {
            }

            public override string GetPrompt() => $"{Hostname}>";
        }
    }
}
