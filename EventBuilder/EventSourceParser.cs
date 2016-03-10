using System;
using System.Collections.Generic;
using System.Reflection;
using EventBuilder.Attributes;
using EventBuilder.Events;
using EventBuilder.Utility;

namespace EventBuilder
{
  public static class EventSourceParser
  {
    public static EventSourceMetaData Parse(Type type)
    {
      object[] eventSourceTypeAttributes = type.GetCustomAttributes(typeof(EventSourceTypeAttribute), false);
      if(eventSourceTypeAttributes.Length==0)
      {
        Console.WriteLine(type);
        object[] types = type.GetCustomAttributes(false);
        foreach (var o in types)
        {
          Console.WriteLine(o);
        }
        
        throw new Exception("Invalid EventSource Type: No EventSourceTypeAttribute specified");
      }
      EventSourceTypeAttribute eventSourceTypeAttribute = (EventSourceTypeAttribute)eventSourceTypeAttributes[0] ;

      MethodInfo[] methods = type.GetMethods();
      List<EventMetaData> declaredEventTypes = new List<EventMetaData>();
      foreach (MethodInfo method in methods)
      {
        object[] eventTypeAttributes = method.GetCustomAttributes(typeof (EventTypeAttribute), false);
        if (eventTypeAttributes.Length == 0)
        {
          throw new Exception(string.Format("Invalid EventSource Type: method {0} doesn't have event type specified",method.Name));
        }
        EventTypeAttribute eventTypeAttribute = (EventTypeAttribute)eventTypeAttributes[0];
        EventType eventType = new EventType(eventSourceTypeAttribute.Name,eventTypeAttribute.Name)
                                {
                                  Level = TranslateLevel(eventTypeAttribute.Level),
                                  MessageTemplate = eventTypeAttribute.MessageTemplate,
                                  PayLoadType = eventTypeAttribute.PayLoadType
                                };
        try
        {
          ValidateMessageTemplate(eventTypeAttribute, method);
          Dictionary<string, int> propertiesMapping = ParsePayLoadProperties(eventTypeAttribute, method);
          declaredEventTypes.Add(new EventMetaData(eventType, propertiesMapping));
        }
        catch(Exception e)
        {
          throw new Exception("Invalid EventSource Type", e);
        }
      }
      return new EventSourceMetaData(eventSourceTypeAttribute,declaredEventTypes);
    }

    

    private static void ValidateMessageTemplate(EventTypeAttribute eventTypeAttribute, MethodInfo methodInfo)
    {
      int requiredParameterCount = StringFormatParser.GetRequiredParameterCount(eventTypeAttribute.MessageTemplate);
      int actualParameterCount = methodInfo.GetParameters().Length;
      if (actualParameterCount < requiredParameterCount)
      {
        throw new Exception(string.Format("Invalid Event.MessageTemplate: template requires {0} parameter(s), method {1} has only {2} parameter(s)",requiredParameterCount,methodInfo.Name,actualParameterCount));
      }
    }

    private static Dictionary<string, int> ParsePayLoadProperties(EventTypeAttribute eventTypeAttribute, MethodInfo methodInfo)
    {
      Dictionary<string, int> property2Parameter=new Dictionary<string, int>();
      if (eventTypeAttribute.PayLoadType!=null)
      {
        if(eventTypeAttribute.PayLoadType.GetConstructor(Type.EmptyTypes)==null)
        {
          throw new Exception(string.Format("Invalid Event.PayLoadType: type {0} has no default constructor", eventTypeAttribute.PayLoadType));          
        }
        ParameterInfo[] parameterInfos = methodInfo.GetParameters();
        for (int i = 0; i < parameterInfos.Length; i++)
        {
          ParameterInfo parameterInfo = parameterInfos[i];
          object[] payLoadPropertyAttributes = parameterInfo.GetCustomAttributes(typeof (PayLoadPropertyAttribute),
                                                                                 false);
          if (payLoadPropertyAttributes.Length == 0)
          {
            continue;
          }
          PayLoadPropertyAttribute payLoadPropertyAttribute = (PayLoadPropertyAttribute) payLoadPropertyAttributes[0];
          if (property2Parameter.ContainsKey(payLoadPropertyAttribute.Name))
          {
            throw new Exception(
              string.Format("Invalid Event.PayLoadType property mapping: Duplicate mapping for payload property {0}",
                            payLoadPropertyAttribute.Name));
          }
          try
          {
            ValidatePayLoadPropertyAttribute(payLoadPropertyAttribute, parameterInfo, eventTypeAttribute.PayLoadType);
          }
          catch (Exception e)
          {
            throw new Exception(string.Format("Invalid Event.PayLoadType property mapping failed: {0}", e), e);
          }
          property2Parameter.Add(payLoadPropertyAttribute.Name, i);
        }
      }
      return property2Parameter;
    }

    private static void ValidatePayLoadPropertyAttribute(PayLoadPropertyAttribute payLoadPropertyAttribute, ParameterInfo parameterInfo, Type payLoadType)
    {
      PropertyInfo propertyInfo = payLoadType.GetProperty(payLoadPropertyAttribute.Name);
      if (propertyInfo == null )
      {
        throw new Exception(string.Format("No property {0} defined in type {1}", payLoadPropertyAttribute.Name, payLoadType));
      }
      if (!propertyInfo.CanWrite)
      {
        throw new Exception(string.Format("Property {0} in type {1} is not writable", payLoadPropertyAttribute.Name, payLoadType));        
      }
      if(!propertyInfo.PropertyType.IsAssignableFrom(parameterInfo.ParameterType))
      {
        throw new Exception(string.Format("Property {0} in type {1} is not compatible with parameter {2}", payLoadPropertyAttribute.Name, payLoadType,parameterInfo.Name));                
      }
    }

    private static Events.EventLevel TranslateLevel(Attributes.EventLevel level)
    {
      return (Events.EventLevel) Enum.Parse(typeof (Events.EventLevel), level.ToString());
    }
  }
}
