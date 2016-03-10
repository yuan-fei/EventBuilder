using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventBuilder;
using EventBuilder.Events;
using EventBuilder.Events.Consumers;
using EventBuilder.Events.Filters;

namespace TestEventBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            //Configure consumer
            EventBroker.Instance.AddEventConsumers(new FileConsumer(1),
              new ConsoleConsumer() {EventFilter = new BaseFilter(e => e.Level == EventLevel.Info)});

            //Create event source
            var evtSource = EventBroker.Instance.EventSourceContainer.GetEventSource<ICardEventSource>();

            //Raise event
            evtSource.CardInserted(1, "123456");
            evtSource.CardRemoved();

            Console.ReadLine();
        }
    }
}
