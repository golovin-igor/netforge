using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Engine;

public interface ISimulationMonitor
{
    void RegisterMetric(string name, Func<double> valueProvider);
    void UnregisterMetric(string name);
    IReadOnlyDictionary<string, double> GetCurrentMetrics();
    ITimeSeries GetMetricHistory(string name, TimeSpan duration);
    void ExportMetrics(string filename, MetricFormat format);
}
