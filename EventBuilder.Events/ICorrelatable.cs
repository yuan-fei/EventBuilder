namespace EventBuilder.Events
{
  public interface ICorrelatable
  {
    IIdentifier CorrelationId { get; }
  }
}
