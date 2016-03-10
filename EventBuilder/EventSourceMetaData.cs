using System.Collections.Generic;
using EventBuilder.Attributes;
using EventBuilder.Events;

namespace EventBuilder
{
  public class EventSourceMetaData
  {
    public EventSourceMetaData(EventSourceTypeAttribute eventSourceType, List<EventMetaData> events)
    {
      Events = events;
      EventSourceType = eventSourceType;
    }

    public EventSourceTypeAttribute EventSourceType { get; set; }
    public List<EventMetaData> Events{ get; private set; }
  }

  public class EventMetaData
  {
    public EventMetaData(EventType eventType, Dictionary<string, int> payLoadPropertiesMapping)
    {
      EventType = eventType;
      PayLoadPropertiesMapping = payLoadPropertiesMapping;
    }
    public EventType EventType { get; set; }
    public Dictionary<string, int> PayLoadPropertiesMapping{ get; set; }
  }
}
