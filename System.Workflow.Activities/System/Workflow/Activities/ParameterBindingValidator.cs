namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal static class ParameterBindingValidator
    {
        internal static ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();
            Activity activity = obj as Activity;
            if (!(activity is CallExternalMethodActivity) && !(activity is HandleExternalEventActivity))
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(Activity).FullName }), "obj");
            }
            Type type = (activity is CallExternalMethodActivity) ? ((CallExternalMethodActivity) activity).InterfaceType : ((HandleExternalEventActivity) activity).InterfaceType;
            if (type != null)
            {
                string str = (activity is CallExternalMethodActivity) ? ((CallExternalMethodActivity) activity).MethodName : ((HandleExternalEventActivity) activity).EventName;
                if (string.IsNullOrEmpty(str))
                {
                    return validationErrors;
                }
                WorkflowParameterBindingCollection parameterBindings = (activity is CallExternalMethodActivity) ? ((CallExternalMethodActivity) activity).ParameterBindings : ((HandleExternalEventActivity) activity).ParameterBindings;
                MethodInfo method = type.GetMethod(str);
                if ((method == null) && (activity is CallExternalMethodActivity))
                {
                    return validationErrors;
                }
                bool isEvent = false;
                if (method == null)
                {
                    EventInfo eventInfo = type.GetEvent(str);
                    if ((eventInfo == null) || (eventInfo.GetAddMethod(true) == null))
                    {
                        return validationErrors;
                    }
                    Type eventHandlerType = eventInfo.EventHandlerType;
                    if (eventHandlerType == null)
                    {
                        eventHandlerType = TypeProvider.GetEventHandlerType(eventInfo);
                    }
                    method = eventHandlerType.GetMethod("Invoke");
                    isEvent = true;
                }
                ValidateParameterBinding(manager, activity, isEvent, str, method, parameterBindings, validationErrors);
            }
            return validationErrors;
        }

        private static void ValidateParameterBinding(ValidationManager manager, Activity activity, bool isEvent, string operation, MethodInfo mInfo, WorkflowParameterBindingCollection parameterBindings, ValidationErrorCollection validationErrors)
        {
            Hashtable hashtable = new Hashtable();
            ParameterInfo[] parameters = mInfo.GetParameters();
            bool flag = false;
            foreach (ParameterInfo info in parameters)
            {
                if (TypeProvider.IsAssignable(typeof(ExternalDataEventArgs), info.ParameterType))
                {
                    if (info.Position == 1)
                    {
                        flag = true;
                    }
                    ValidateParameterSerializabiltiy(validationErrors, info.ParameterType);
                }
                hashtable.Add(info.Name, info);
            }
            if (isEvent && (!flag || (parameters.Length != 2)))
            {
                validationErrors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_InvalidEventArgsSignature", new object[] { operation }), new object[0]), 0x120, false, "EventName"));
            }
            if (mInfo.ReturnType != typeof(void))
            {
                hashtable.Add("(ReturnValue)", mInfo.ReturnParameter);
            }
            foreach (WorkflowParameterBinding binding in parameterBindings)
            {
                string parameterName = binding.ParameterName;
                if (!hashtable.ContainsKey(parameterName))
                {
                    if (isEvent)
                    {
                        validationErrors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_InvalidEventPropertyName", new object[] { parameterName }), new object[0]), 0x120, false, "ParameterBindings"));
                    }
                    else
                    {
                        validationErrors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_InvalidMethodPropertyName", new object[] { parameterName }), new object[0]), 0x120, false, "ParameterBindings"));
                    }
                }
                else
                {
                    object obj2 = null;
                    if (binding.IsBindingSet(WorkflowParameterBinding.ValueProperty))
                    {
                        obj2 = binding.GetBinding(WorkflowParameterBinding.ValueProperty);
                    }
                    else
                    {
                        obj2 = binding.GetValue(WorkflowParameterBinding.ValueProperty);
                    }
                    if (obj2 != null)
                    {
                        ParameterInfo info2 = hashtable[parameterName] as ParameterInfo;
                        if (info2 != null)
                        {
                            AccessTypes read = AccessTypes.Read;
                            if (info2.IsOut || info2.IsRetval)
                            {
                                read = AccessTypes.Write;
                            }
                            else if (info2.ParameterType.IsByRef)
                            {
                                read |= AccessTypes.Write;
                            }
                            ValidationErrorCollection errors = System.Workflow.Activities.Common.ValidationHelpers.ValidateProperty(manager, activity, obj2, new PropertyValidationContext(binding, null, parameterName), new BindValidationContext(info2.ParameterType.IsByRef ? info2.ParameterType.GetElementType() : info2.ParameterType, read));
                            validationErrors.AddRange(errors);
                        }
                    }
                }
            }
        }

        private static void ValidateParameterSerializabiltiy(ValidationErrorCollection validationErrors, Type type)
        {
            object[] customAttributes = type.GetCustomAttributes(typeof(SerializableAttribute), false);
            Type type2 = type.GetInterface(typeof(ISerializable).FullName);
            if ((customAttributes.Length == 0) && (type2 == null))
            {
                validationErrors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_EventArgumentValidationException"), new object[] { type.FullName }), 0x120, false, "EventName"));
            }
        }
    }
}

