namespace System.Workflow.Activities
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Workflow.ComponentModel;

    internal static class InvokeHelper
    {
        internal static object CloneOutboundValue(object source, BinaryFormatter formatter, string name)
        {
            if ((source == null) || source.GetType().IsValueType)
            {
                return source;
            }
            ICloneable cloneable = source as ICloneable;
            if (cloneable != null)
            {
                return cloneable.Clone();
            }
            MemoryStream serializationStream = new MemoryStream(0x400);
            try
            {
                formatter.Serialize(serializationStream, source);
            }
            catch (SerializationException exception)
            {
                throw new InvalidOperationException(SR.GetString("Error_CallExternalMethodArgsSerializationException", new object[] { name }), exception);
            }
            serializationStream.Position = 0L;
            return formatter.Deserialize(serializationStream);
        }

        internal static object[] GetParameters(MethodBase methodBase, WorkflowParameterBindingCollection parameterBindings)
        {
            ParameterInfo[] parameters = methodBase.GetParameters();
            object[] objArray = new object[parameters.Length];
            int index = 0;
            foreach (ParameterInfo info in parameters)
            {
                if (parameterBindings.Contains(info.Name))
                {
                    WorkflowParameterBinding binding = parameterBindings[info.Name];
                    objArray[index] = binding.Value;
                }
                index++;
            }
            return objArray;
        }

        internal static object[] GetParameters(MethodBase methodBase, WorkflowParameterBindingCollection parameterBindings, out ParameterModifier[] parameterModifiers)
        {
            ParameterInfo[] parameters = methodBase.GetParameters();
            object[] objArray = new object[parameters.Length];
            if (objArray.Length == 0)
            {
                parameterModifiers = new ParameterModifier[0];
                return objArray;
            }
            int index = 0;
            BinaryFormatter formatter = null;
            ParameterModifier modifier = new ParameterModifier(objArray.Length);
            foreach (ParameterInfo info in parameters)
            {
                if (info.ParameterType.IsByRef)
                {
                    modifier[index] = true;
                }
                else
                {
                    modifier[index] = false;
                }
                if (parameterBindings.Contains(info.Name))
                {
                    WorkflowParameterBinding binding = parameterBindings[info.Name];
                    if (formatter == null)
                    {
                        formatter = new BinaryFormatter();
                    }
                    objArray[index] = CloneOutboundValue(binding.Value, formatter, info.Name);
                }
                index++;
            }
            ParameterModifier[] modifierArray = new ParameterModifier[] { modifier };
            parameterModifiers = modifierArray;
            return objArray;
        }

        internal static void InitializeParameters(MethodInfo methodBase, WorkflowParameterBindingCollection parameterBindings)
        {
            foreach (ParameterInfo info in methodBase.GetParameters())
            {
                if (!parameterBindings.Contains(info.Name))
                {
                    parameterBindings.Add(new WorkflowParameterBinding(info.Name));
                }
            }
            if ((methodBase.ReturnType != typeof(void)) && !parameterBindings.Contains("(ReturnValue)"))
            {
                parameterBindings.Add(new WorkflowParameterBinding("(ReturnValue)"));
            }
        }

        internal static void SaveOutRefParameters(object[] actualParameters, MethodBase methodBase, WorkflowParameterBindingCollection parameterBindings)
        {
            int index = 0;
            BinaryFormatter formatter = null;
            foreach (ParameterInfo info in methodBase.GetParameters())
            {
                if (parameterBindings.Contains(info.Name) && (info.ParameterType.IsByRef || (info.IsIn && info.IsOut)))
                {
                    WorkflowParameterBinding binding = parameterBindings[info.Name];
                    if (formatter == null)
                    {
                        formatter = new BinaryFormatter();
                    }
                    binding.Value = CloneOutboundValue(actualParameters[index], formatter, info.Name);
                }
                index++;
            }
        }
    }
}

