namespace EventBuilder.Events
{
  public interface IEventConsumer
  {
    void OnEvent(BaseEvent eventData);
  }
}
