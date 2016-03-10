using System.Collections.Generic;
using EventBuilder.Events.Filters;

namespace EventBuilder.Events
{
  public class EventConsumer
  {
    private static readonly EventConsumer sInstance=new EventConsumer();

    private EventConsumer()
    {
      
    }

    public static EventConsumer Instance
    {
      get { return sInstance; }
    }

    private readonly List<IEventConsumer> mConsumers = new List<IEventConsumer>();

    public void AddEventConsumers(params IEventConsumer[] consumers)
    {
      mConsumers.AddRange(consumers);
    }

    public void RemoveEventConsumer(IEventConsumer consumer)
    {
      mConsumers.Remove(consumer);
    }

    public IFilter Filter { get; set; }

    public void OnEvent(BaseEvent eventData)
    {
      if (Filter==null||Filter.Filter(eventData))
      {
        mConsumers.ForEach(c => c.OnEvent(eventData));
      }
    }
  }
}
