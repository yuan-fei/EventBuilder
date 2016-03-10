namespace EventBuilder.Events
{
  public abstract class BaseEventSource:ICorrelatable
  {
    protected BaseEventSource(IIdentifier correlationId)
    {
      CorrelationId = correlationId;
    }

    protected BaseEventSource()
      : this(NotCorrelated.Instance)
    {
    }

    public IIdentifier CorrelationId { get; private set; }


    protected void BuildAndRaiseEvent(EventType eventType, string message,object eventPayLoad)
    {
      BaseEvent eventData = new BaseEvent
      {
        Category = eventType.Category,
        Name = eventType.Name,
        CorrelationId = CorrelationId.Id,
        Level = eventType.Level,
        Message = message,
        Payload = eventPayLoad
      };
      RaiseEvent(eventData);
    }

    protected void RaiseEvent(BaseEvent eventData)
    {
      //Console.WriteLine(eventData.EventType.Name);
      EventConsumer.Instance.OnEvent(eventData);
    }
  }
}
