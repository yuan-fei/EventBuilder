namespace EventBuilder.Events
{
  public interface IEventSourceContainer
  {
    void RegisterEventSource<T, U>();
    T GetEventSource<T>();
    T GetEventSource<T>(IIdentifier correlationId);
  }
}
