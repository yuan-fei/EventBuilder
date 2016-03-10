using System;

namespace EventBuilder.Attributes
{
  [AttributeUsage(AttributeTargets.Method)]
  public class EventTypeAttribute:Attribute
  {
    public EventTypeAttribute(string name)
    {
      Name = name;
      Level = EventLevel.Debug;
    }
    public string Name { get; set; }
    public EventLevel Level { get; set; }
    public string MessageTemplate { get; set; }
    public Type PayLoadType { get; set; }
  }
}