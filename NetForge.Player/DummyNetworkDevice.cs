// TODO: Enhance dummy device for testing and capability scanning
// This class provides a minimal network device implementation for utility purposes

using NetForge.Simulation.Common;

namespace NetForge.Player;

/// <summary>
/// Dummy network device used for capability scanning and testing
/// </summary>
public class DummyNetworkDevice() : NetworkDevice("DummyDevice")
{
    protected override void InitializeDefaultInterfaces()
    {
    }

    public override string GetPrompt()
    {
        return "DummyDevice>";
    }

    protected override void RegisterDeviceSpecificHandlers()
    {
    }
}