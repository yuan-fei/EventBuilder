using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using EventBuilder.Events;
using EventBuilder.Utility;

namespace EventBuilder
{
  public class EventSourceTypeBuilder
  {
    private const string EventSourceAssemblyName = "EventSourceImplementationAssembly";
    private const string EventSourceModuleName = "EventSourceImplementationModule";
    private const string EventSourceTypeNameTemplate = "{0}";
    private const string DeclaredEventsFieldName = "mDeclaredEvents";

    private const FieldAttributes EventTypeFieldAttributes =
      FieldAttributes.Private | FieldAttributes.InitOnly | FieldAttributes.Static;
    private const MethodAttributes MethodImplementationAttributes=MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final| MethodAttributes.HideBySig | MethodAttributes.NewSlot;
    private static readonly OpCode[] LoadConstOpCodes = {
            OpCodes.Ldc_I4_0,
            OpCodes.Ldc_I4_1,
            OpCodes.Ldc_I4_2,
            OpCodes.Ldc_I4_3,
            OpCodes.Ldc_I4_4,
            OpCodes.Ldc_I4_5,
            OpCodes.Ldc_I4_6,
            OpCodes.Ldc_I4_7,
            OpCodes.Ldc_I4_8,
        };
    private static readonly OpCode[] LoadArgOpCodes = {
            OpCodes.Ldarg_1,
            OpCodes.Ldarg_2,
            OpCodes.Ldarg_3,
        };
    private static readonly MethodInfo GetEventTypesMethodInfo = ConstructGetEventTypesMethodInfo();
    private static readonly MethodInfo CreateAndRaiseEventMethodInfo = ConstructBuildAndRaiseEventMethodInfo();
    private static readonly MethodInfo StringFormatMethodInfo = ConstructStringFormatMethofInfo();
    private static AssemblyBuilder assemblyBuilder;

    private readonly Type mEventSourceInterfaceType;
    private TypeBuilder mEventSourceTypeBuilder;
    private FieldBuilder mDeclaredEventsFieldBuilder;
    static EventSourceTypeBuilder()
    {
      assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(EventSourceAssemblyName), AssemblyBuilderAccess.RunAndSave);
    }

    public EventSourceTypeBuilder(Type interfaceType)
    {
      mEventSourceInterfaceType = interfaceType;
    }

    public Type CreateEventSourceType()
    {
      return CreateEventSourceType(mEventSourceInterfaceType);
    }

    private Type CreateEventSourceType(Type eventSourceInterfaceType)
    {
      EventSourceMetaData eventSourceMetaData = EventSourceParser.Parse(eventSourceInterfaceType);
      EventTypeRegistry.Instance.AddEventTypesByCategory(eventSourceMetaData.EventSourceType.Name, eventSourceMetaData.Events.Select(e=>e.EventType).ToArray());

      ModuleBuilder moduleBuilder = GetModuleBuilder();
      mEventSourceTypeBuilder =
        moduleBuilder.DefineType(GetEventSourceImplementationTypeName(eventSourceMetaData.EventSourceType.Name),TypeAttributes.Public | TypeAttributes.Class, typeof (BaseDynamicEventSource),new Type[]{eventSourceInterfaceType});

      mDeclaredEventsFieldBuilder = mEventSourceTypeBuilder.DefineField(DeclaredEventsFieldName,typeof(EventType[]),EventTypeFieldAttributes);
      
      AddStaticConstructor(eventSourceMetaData);

      AddConstructors();

      AddInterfaceMethodImplementations(eventSourceInterfaceType, eventSourceMetaData);

      Type implementationType = mEventSourceTypeBuilder.CreateType();
#if DEBUG_SAVE_GENERATED_ASSEMBLY
      assemblyBuilder.Save(EventSourceAssemblyName + ".dll");
#endif
      return implementationType;
    }

    private void AddInterfaceMethodImplementations(Type eventSourceInterfaceType, EventSourceMetaData eventSourceMetaData)
    {
      for (int i = 0; i < eventSourceInterfaceType.GetMethods().Length; i++)
      {
        MethodInfo interfaceMethod = eventSourceInterfaceType.GetMethods()[i];
        ParameterInfo[] parameterInfos = interfaceMethod.GetParameters();
        MethodBuilder methodBuilder = mEventSourceTypeBuilder.DefineMethod(interfaceMethod.Name,
                                                                           MethodImplementationAttributes, typeof (void),
                                                                           parameterInfos.Select(p => p.ParameterType).
                                                                             ToArray());
        for (int j = 0; j < parameterInfos.Length; j++)
        {
          ParameterInfo parameterInfo = parameterInfos[j];
          methodBuilder.DefineParameter(j + 1, ParameterAttributes.None, parameterInfo.Name);
        }

        ILGenerator il = methodBuilder.GetILGenerator();
        EventMetaData eventMetaData = eventSourceMetaData.Events[i];
        
        //declare 'payLoad'
        LocalBuilder payLoadVariable = il.DeclareLocal(typeof(object));
        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Stloc, payLoadVariable);

        //declare arg array for message
        LocalBuilder messageArgVariable = il.DeclareLocal(typeof(object[]));
        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Stloc, messageArgVariable);

        //This
        EmitLoadThis(il);

        //EventType
        il.Emit(OpCodes.Ldsfld, mDeclaredEventsFieldBuilder);
        EmitLoadConst(il, i);
        il.Emit(OpCodes.Ldelem_Ref);

        //Message
        EmitMessage(eventMetaData, il, messageArgVariable,parameterInfos);

        //PayLoad
        EmitPayLoadCreation(eventMetaData, il, payLoadVariable);
        il.Emit(OpCodes.Ldloc, payLoadVariable);
        
        il.EmitCall(OpCodes.Callvirt, CreateAndRaiseEventMethodInfo, new Type[] {typeof (EventType), typeof (object)});
        il.Emit(OpCodes.Ret);
      }
    }


    private void EmitMessage(EventMetaData eventMetaData, ILGenerator il, LocalBuilder messageArgVariable, ParameterInfo[] parameterInfos)
    {
      int argLength = parameterInfos.Length;
      if (string.IsNullOrEmpty(eventMetaData.EventType.MessageTemplate))
      {
        il.Emit(OpCodes.Ldnull);
      }
      else
      {
        il.Emit(OpCodes.Ldstr, eventMetaData.EventType.MessageTemplate);
        EmitLoadConst(il, argLength);
        il.Emit(OpCodes.Newarr, typeof(object));
        il.Emit(OpCodes.Stloc, messageArgVariable);
        for (int i = 0; i < argLength; i++)
        {
          il.Emit(OpCodes.Ldloc, messageArgVariable);
          EmitLoadConst(il, i);
          EmitNonStaticLoadArg(il, i);
          Type parameterType = parameterInfos[i].ParameterType;
          if (parameterType.IsValueType)
          {
            il.Emit(OpCodes.Box, parameterType);
          }
          il.Emit(OpCodes.Stelem_Ref);
        }
        il.Emit(OpCodes.Ldloc, messageArgVariable);
        il.Emit(OpCodes.Call, StringFormatMethodInfo);
      }
    }

    private static void EmitPayLoadCreation(EventMetaData eventMetaData, ILGenerator il, LocalBuilder payLoadVariable)
    {
      if (eventMetaData.EventType.PayLoadType != null)
      {
        ConstructorInfo con = eventMetaData.EventType.PayLoadType.GetConstructor(Type.EmptyTypes);
        if (con != null)
        {
          il.Emit(OpCodes.Newobj, con);
          il.Emit(OpCodes.Stloc, payLoadVariable);
        }
        else
        {
          //should not arrive here
          throw new Exception(string.Format("Event PayLoad type {0} has no default constructor",
                                            eventMetaData.EventType.PayLoadType));
        }
        // set payLoad's property
        if (eventMetaData.PayLoadPropertiesMapping != null && eventMetaData.PayLoadPropertiesMapping.Count>0)
        {
          
          foreach (KeyValuePair<string, int> payLoad2Parameter in eventMetaData.PayLoadPropertiesMapping)
          {
            il.Emit(OpCodes.Ldloc, payLoadVariable);
            EmitNonStaticLoadArg(il, payLoad2Parameter.Value);
            PropertyInfo property = eventMetaData.EventType.PayLoadType.GetProperty(payLoad2Parameter.Key);
            MethodInfo propertySetMethod = property.GetSetMethod();
            ParameterInfo[] propertySetMethodParameterInfos = propertySetMethod.GetParameters();
            Type[] parameterTypes = (propertySetMethodParameterInfos.Length > 0)
                                      ? propertySetMethodParameterInfos.Select(p => p.ParameterType).ToArray()
                                      : Type.EmptyTypes;
            il.EmitCall(OpCodes.Callvirt, propertySetMethod, parameterTypes);
          }
        }

      }
      
    }

    private void AddConstructors()
    {
      ConstructorInfo[] baseConstructors = typeof (BaseDynamicEventSource).GetConstructors(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance);

      foreach (ConstructorInfo baseConstructor in baseConstructors)
      {
        ParameterInfo[] baseConstructorParameterInfos = baseConstructor.GetParameters();
        Type[] baseConstructorParameterTypes = (baseConstructorParameterInfos.Length > 0)
                                                 ? baseConstructorParameterInfos.Select(p => p.ParameterType).ToArray()
                                                 : Type.EmptyTypes;
        ConstructorBuilder instanceConstructorBuilder = mEventSourceTypeBuilder.DefineConstructor(MethodAttributes.Public,
                                                                                                  CallingConventions.HasThis,
                                                                                                  baseConstructorParameterTypes);
        //define constructor parameters
        for (int i = 0; i < baseConstructorParameterInfos.Length; i++)
        {
          ParameterInfo baseConstructorParameterInfo = baseConstructorParameterInfos[i];
          instanceConstructorBuilder.DefineParameter(i + 1, ParameterAttributes.None,
                                                     baseConstructorParameterInfo.Name);
        }
        ILGenerator il = instanceConstructorBuilder.GetILGenerator();

        // call base class construtor
        il.Emit(OpCodes.Ldarg_0);
        for (int i = 0; i < baseConstructorParameterInfos.Length; ++i)
        {
          il.Emit(OpCodes.Ldarg, i + 1);
        }
        il.Emit(OpCodes.Call, baseConstructor);
        il.Emit(OpCodes.Ret);
      }
    }

    private void AddStaticConstructor(EventSourceMetaData eventSourceMetaData)
    {
      ConstructorBuilder typeConstructorBuilder = mEventSourceTypeBuilder.DefineConstructor(MethodAttributes.Static,
                                                                                            CallingConventions.Standard,
                                                                                            new Type[0]);
      ILGenerator il = typeConstructorBuilder.GetILGenerator();
      il.Emit(OpCodes.Ldstr, eventSourceMetaData.EventSourceType.Name);
      il.EmitCall(OpCodes.Call, GetEventTypesMethodInfo, new[] {typeof (string)});
      il.Emit(OpCodes.Stsfld, mDeclaredEventsFieldBuilder);
      il.Emit(OpCodes.Ret);
    }

    private static void EmitLoadThis(ILGenerator il)
    {
      il.Emit(OpCodes.Ldarg_0);
    }

    private static void EmitNonStaticLoadArg(ILGenerator il, int index)
    {
      if (index < LoadArgOpCodes.Length)
      {
        il.Emit(LoadArgOpCodes[index]);
      }
      else
      {
        il.Emit(OpCodes.Ldarg, index+1);
      }
    }

    private static void EmitLoadConst(ILGenerator il,int value)
    {
      if(value<LoadConstOpCodes.Length)
      {
        il.Emit(LoadConstOpCodes[value]);
      }
      else
      {
        il.Emit(OpCodes.Ldc_I4,value); 
      }
    }

    private static MethodInfo ConstructGetEventTypesMethodInfo()
    {
      return typeof (BaseDynamicEventSource).GetMethod("GetEventTypes", BindingFlags.Static | BindingFlags.NonPublic);
    }

    private static MethodInfo ConstructBuildAndRaiseEventMethodInfo()
    {
      return typeof(BaseDynamicEventSource).GetMethod("BuildAndRaiseEvent", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    private static MethodInfo ConstructStringFormatMethofInfo()
    {
      return StaticReflection.GetMethodInfo(() => string.Format("{0}{1}{2}{3}", 0, 1, 2, 3));
    }

    private static string GetEventSourceImplementationTypeName(string eventSourceName)
    {
      return string.Format(EventSourceTypeNameTemplate, eventSourceName);
    }

    private static ModuleBuilder GetModuleBuilder()
    {
      ModuleBuilder builder = assemblyBuilder.GetDynamicModule(EventSourceModuleName);
      if (builder == null)
      {
#if DEBUG_SAVE_GENERATED_ASSEMBLY 
        builder = assemblyBuilder.DefineDynamicModule(EventSourceModuleName, EventSourceAssemblyName + ".dll", true);
#else
        builder = assemblyBuilder.DefineDynamicModule(EventSourceModuleName);
#endif
      }
      return builder;
    }

    //private static ModuleBuilder GetModuleBuilder()
    //{
    //  string moduleName = Guid.NewGuid().ToString("N");
    //  ModuleBuilder builder = assemblyBuilder.DefineDynamicModule(moduleName, moduleName + ".dll", true);
    //  return builder;
    //}
  }
}
