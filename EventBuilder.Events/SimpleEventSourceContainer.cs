using System;
using System.Collections.Generic;
using System.Reflection;

namespace EventBuilder.Events
{
  public class SimpleEventSourceContainer:IEventSourceContainer
  {
    private readonly Dictionary<Type,Type> mEventSourceMapping=new Dictionary<Type, Type>();
    public void RegisterEventSource<T, U>()
    {
      if(typeof(T).IsAssignableFrom(typeof(U)))
      {
        //Type U must have a default constructor
        if (!mEventSourceMapping.ContainsKey(typeof(T))&&null!=typeof (U).GetConstructor(Type.EmptyTypes))
        {
          mEventSourceMapping.Add(typeof (T), typeof (U));
        }
      }
    }

    public  T GetEventSource<T>()
    {
      return GetEventSource<T>(NotCorrelated.Instance);
    }

    public T GetEventSource<T>(IIdentifier correlationId)
    {
      if (mEventSourceMapping.ContainsKey(typeof(T)))
      {
        Type impType = mEventSourceMapping[typeof(T)];
        ConstructorInfo defaultConstructor = impType.GetConstructor(Type.EmptyTypes);
        if (defaultConstructor != null)
        {
          return (T)defaultConstructor.Invoke(new object[]{correlationId});
        }
      }
      return default(T);
    }
  }
}
