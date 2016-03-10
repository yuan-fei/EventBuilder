using System;

namespace EventBuilder.Attributes
{
  [AttributeUsage(AttributeTargets.Interface)]
  public class EventSourceTypeAttribute:Attribute
  {
    public EventSourceTypeAttribute(string name)
    {
      Name = name;
    }
    public string Name { get; set; }
  }
}
