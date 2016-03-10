using System;

namespace EventBuilder.Events
{
  public class EventType
  {
    public EventType(string category, string name)
    {
      Category = category;
      Name = name;
    }
    public string Category { get; set; }
    public string Name { get; set; }
    public EventLevel Level { get; set; }
    public string MessageTemplate { get; set; }
    public Type PayLoadType { get; set; }

  }
}
