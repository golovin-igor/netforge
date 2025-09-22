namespace NetForge.SimulationModel.Types;

public enum ApplicationEventType
{
    ServiceStarted,
    ServiceStopped,
    RequestReceived,
    ResponseSent,
    AuthenticationSuccess,
    AuthenticationFailure,
    ConfigurationChanged
}
