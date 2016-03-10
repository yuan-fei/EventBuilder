using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventBuilder.Events;

namespace EventBuilder
{
  public abstract class BaseDynamicEventSource:BaseEventSource
  {
    protected BaseDynamicEventSource(IIdentifier correlationId):base(correlationId)
    {
      
    }

    protected BaseDynamicEventSource():base(){}

    protected static EventType[] GetEventTypes(string category)
    {
      return EventTypeRegistry.Instance.GetEventTypesByCategory(category);
    }
  }
}
