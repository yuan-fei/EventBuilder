using System.Collections.Generic;
using EventBuilder.Events;
using EventBuilder.Events.Filters;

namespace EventBuilder
{
  public class EventBroker
  {
    private static readonly EventBroker sInstance=new EventBroker();

    private EventBroker()
    {
      
    }

    public static EventBroker Instance
    {
      get { return sInstance; }
    }

    private readonly IEventSourceContainer mEventSourceContainer = new EventSourceContainer();
    public IEventSourceContainer EventSourceContainer
    {
      get
      {
        return mEventSourceContainer;
      }
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
