namespace EventBuilder.Events
{
  public class EventSourceLocator
  {
    private readonly IEventSourceContainer mEventSourceContainer;

    public EventSourceLocator(IEventSourceContainer eventSourceContainer)
    {
      mEventSourceContainer = eventSourceContainer;
    }

    public T GetEventSource<T>()
    {
      return mEventSourceContainer.GetEventSource<T>();
    }

    public T GetEventSource<T>(IIdentifier correlationId)
    {
      return mEventSourceContainer.GetEventSource<T>(correlationId);
    }
  }
}
