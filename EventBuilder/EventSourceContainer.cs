using EventBuilder.Events;

namespace EventBuilder
{
  internal class EventSourceContainer:IEventSourceContainer
  {

    public void RegisterEventSource<T, U>()
    {
      EventSourceFactory.Register(typeof(T),typeof(U));
    }

    public T GetEventSource<T>()
    {
      return (T)EventSourceFactory.Create(typeof (T), NotCorrelated.Instance);
    }

    public T GetEventSource<T>(IIdentifier correlationId)
    {
      return (T)EventSourceFactory.Create(typeof(T), correlationId);
    }
  }
}
