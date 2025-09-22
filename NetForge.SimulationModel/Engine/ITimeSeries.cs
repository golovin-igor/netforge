namespace NetForge.SimulationModel.Engine;

public interface ITimeSeries
{
    double GetValueAt(double time);

    double StartTime { get; }

    double EndTime { get; }

    ITimeSeries Clone();

}
