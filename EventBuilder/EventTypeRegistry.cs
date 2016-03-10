using System.Collections.Concurrent;
using EventBuilder.Events;

namespace EventBuilder
{
  public class EventTypeRegistry
  {
    private static readonly EventTypeRegistry sInstance = new EventTypeRegistry();
    private readonly ConcurrentDictionary<string,EventType[]> mRegistry=new ConcurrentDictionary<string, EventType[]>();
    public static EventTypeRegistry Instance { get { return sInstance; } }

    public void AddEventTypesByCategory(string category, EventType[] eventTypes)
    {
      mRegistry.TryAdd(category, eventTypes);
    }

    public EventType[] GetEventTypesByCategory(string category)
    {
      EventType[] eventTypes;
      if( mRegistry.TryGetValue(category, out eventTypes))
      {
        return eventTypes;
      }
      return new EventType[0];
    }
  }
}
