namespace EventBuilder.Events.Filters
{
  public interface IFilter
  {
    bool Filter(BaseEvent eventData);
  }
}
