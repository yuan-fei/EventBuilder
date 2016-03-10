using System;

namespace EventBuilder.Attributes
{
  [AttributeUsage(AttributeTargets.Parameter)]
  public class PayLoadPropertyAttribute : Attribute
  {
    public PayLoadPropertyAttribute(string name)
    {
      Name = name;
    }
    public string Name { get; set; }
  }
}
