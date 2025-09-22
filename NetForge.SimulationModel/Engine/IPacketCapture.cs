using NetForge.SimulationModel.Core;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Engine;

public interface IPacketCapture
{
    void StartCapture(string deviceId, string interfaceId);
    void StopCapture();
    IEnumerable<ICapturedPacket> GetCapturedPackets();
    void SaveCapture(string filename, CaptureFormat format);
    void ApplyFilter(string filterExpression);
}
