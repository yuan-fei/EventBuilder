using System;

namespace EventBuilder.Events
{

  public class BaseEvent
  {
    public string CorrelationId { get; set; }
    public string Category { get; set; }
    public string Name { get; set; }
    public EventLevel Level { get; set; }
    public DateTime Time { get; set; }
    public string Message { get; set; }
    public object Payload { get; set; }

    public BaseEvent()
    {
      Time = DateTime.Now;
    }
  }
}
