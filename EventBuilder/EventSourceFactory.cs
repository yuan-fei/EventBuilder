using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using EventBuilder.Events;

namespace EventBuilder
{
  internal delegate object CreateEventSourceInstance(IIdentifier correlationId);
  public class EventSourceFactory
  {
    private const string CreateEventSourceMethodPattern = "CreateEventSource_{0}";
    private static readonly Dictionary<Type, DynamicEventSourceInstanceCreator> sEventSourceMapping = new Dictionary<Type, DynamicEventSourceInstanceCreator>();
    private static readonly object syncLock = new object();
    public static object Create(Type interfaceType,IIdentifier correlatinId)
    {
      
      if(!sEventSourceMapping.ContainsKey(interfaceType))
      {
        lock (syncLock)
        {
          if (!sEventSourceMapping.ContainsKey(interfaceType))
          {
            Type implementationType = new EventSourceTypeBuilder(interfaceType).CreateEventSourceType();
            RegisterInternal(interfaceType, implementationType);
          }
        }
      }
      DynamicEventSourceInstanceCreator creator=sEventSourceMapping[interfaceType];
      return creator.CreateInstance(correlatinId);
    }

    public static void Register(Type interfaceType, Type implementationType)
    {
      lock (syncLock)
      {
        RegisterInternal(interfaceType, implementationType);
      }
    }

    private static void RegisterInternal(Type interfaceType, Type implementationType)
    {
      CreateEventSourceInstance createMethod = CreateBuilderMethod(implementationType);
      DynamicEventSourceInstanceCreator creator = new DynamicEventSourceInstanceCreator(implementationType, createMethod);
      sEventSourceMapping.Add(interfaceType, creator);
    }

    private static string GetMethodName(string typeName)
    {
      string clearedName = typeName.Replace(',', '_');
      return string.Format(CreateEventSourceMethodPattern, clearedName);
    }

    private static DynamicMethod CreateDynamicMethod(Type implementationType)
    {
      return new DynamicMethod(GetMethodName(implementationType.Name), typeof(object), new Type[] { typeof(IIdentifier) }, true);
    }

    private static CreateEventSourceInstance CreateBuilderMethod(Type implementationType)
    {
      DynamicMethod method = CreateDynamicMethod(implementationType);
      ILGenerator il = method.GetILGenerator();
      il.Emit(OpCodes.Ldarg_0);
      ConstructorInfo con = implementationType.GetConstructor(new Type[] {typeof (IIdentifier)});
      il.Emit(OpCodes.Newobj, con);
      il.Emit(OpCodes.Ret);
      return (CreateEventSourceInstance)method.CreateDelegate(typeof(CreateEventSourceInstance));
    }
  }

  class DynamicEventSourceInstanceCreator
  {
    public DynamicEventSourceInstanceCreator(Type implementationType, CreateEventSourceInstance createEventSourceInstance)
    {
      ImplementationType = implementationType;
      CreateEventSourceInstance = createEventSourceInstance;
    }

    private Type ImplementationType { get; set; }
    private CreateEventSourceInstance CreateEventSourceInstance { get; set; }
    public object CreateInstance(IIdentifier correlationId)
    {
      return CreateEventSourceInstance(correlationId);
      
    }
  }
  
}
