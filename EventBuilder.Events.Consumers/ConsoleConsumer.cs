using System;
using EventBuilder.Events.Filters;

namespace EventBuilder.Events.Consumers
{
  public class ConsoleConsumer : IEventConsumer
  {
    public IFilter EventFilter { get; set; }

    public void OnEvent(BaseEvent eventData)
    {
      if (EventFilter==null||EventFilter.Filter(eventData))
      {
        Console.WriteLine(string.Format("{4} [{0}][{1}-{2}]: {3}", eventData.CorrelationId, eventData.Category,
                                        eventData.Name, eventData.Message, eventData.Time));
        Console.WriteLine();
      }
    }

    
  }


}
