namespace NetForge.SimulationModel.Management;

public interface IMibModule
{
    string GetMibData(string oid);
    void SetMibData(string oid, string value);

}
