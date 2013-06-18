namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class WebServiceResponseValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            WebServiceOutputActivity activity = obj as WebServiceOutputActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(WebServiceOutputActivity).FullName }), "obj");
            }
            if (!Helpers.IsActivityLocked(activity))
            {
                List<ParameterInfo> list;
                List<ParameterInfo> list2;
                WebServiceInputActivity activity2 = null;
                if (string.IsNullOrEmpty(activity.InputActivityName))
                {
                    errors.Add(ValidationError.GetNotSetValidationError("InputActivityName"));
                    return errors;
                }
                ITypeProvider service = (ITypeProvider) manager.GetService(typeof(ITypeProvider));
                if (service == null)
                {
                    throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
                }
                bool flag = false;
                foreach (Activity activity3 in WebServiceActivityHelpers.GetPreceedingActivities(activity))
                {
                    if (((activity3 is WebServiceOutputActivity) && (string.Compare(((WebServiceOutputActivity) activity3).InputActivityName, activity.InputActivityName, StringComparison.Ordinal) == 0)) || ((activity3 is WebServiceFaultActivity) && (string.Compare(((WebServiceFaultActivity) activity3).InputActivityName, activity.InputActivityName, StringComparison.Ordinal) == 0)))
                    {
                        if (activity3 is WebServiceOutputActivity)
                        {
                            errors.Add(new ValidationError(SR.GetString("Error_DuplicateWebServiceResponseFound", new object[] { activity3.QualifiedName, activity.InputActivityName }), 0x56a));
                            return errors;
                        }
                        errors.Add(new ValidationError(SR.GetString("Error_DuplicateWebServiceFaultFound", new object[] { activity3.QualifiedName, activity.InputActivityName }), 0x574));
                        return errors;
                    }
                }
                foreach (Activity activity4 in WebServiceActivityHelpers.GetPreceedingActivities(activity))
                {
                    if (string.Compare(activity4.QualifiedName, activity.InputActivityName, StringComparison.Ordinal) == 0)
                    {
                        if (activity4 is WebServiceInputActivity)
                        {
                            activity2 = activity4 as WebServiceInputActivity;
                            flag = true;
                            break;
                        }
                        flag = false;
                        errors.Add(new ValidationError(SR.GetString("Error_WebServiceReceiveNotValid", new object[] { activity.InputActivityName }), 0x564));
                        return errors;
                    }
                }
                if (!flag)
                {
                    errors.Add(new ValidationError(SR.GetString("Error_WebServiceReceiveNotFound", new object[] { activity.InputActivityName }), 0x55e));
                    return errors;
                }
                Type interfaceType = null;
                if (activity2.InterfaceType != null)
                {
                    interfaceType = service.GetType(activity2.InterfaceType.AssemblyQualifiedName);
                }
                if (interfaceType == null)
                {
                    errors.Add(new ValidationError(SR.GetString("Error_WebServiceReceiveNotConfigured", new object[] { activity2.Name }), 0x566));
                    return errors;
                }
                if (string.IsNullOrEmpty(activity2.MethodName))
                {
                    errors.Add(new ValidationError(SR.GetString("Error_WebServiceReceiveNotConfigured", new object[] { activity2.Name }), 0x566));
                    return errors;
                }
                MethodInfo interfaceMethod = Helpers.GetInterfaceMethod(interfaceType, activity2.MethodName);
                if (interfaceMethod == null)
                {
                    errors.Add(new ValidationError(SR.GetString("Error_WebServiceReceiveNotConfigured", new object[] { activity2.Name }), 0x566));
                    return errors;
                }
                ValidationErrorCollection errors2 = WebServiceActivityHelpers.ValidateParameterTypes(interfaceMethod);
                if (errors2.Count > 0)
                {
                    foreach (ValidationError error in errors2)
                    {
                        error.PropertyName = "InputActivityName";
                    }
                    errors.AddRange(errors2);
                    return errors;
                }
                WebServiceActivityHelpers.GetParameterInfo(interfaceMethod, out list, out list2);
                if (list2.Count == 0)
                {
                    errors.Add(new ValidationError(SR.GetString("Error_WebServiceResponseNotNeeded"), 0x565));
                    return errors;
                }
                foreach (ParameterInfo info2 in list2)
                {
                    string name = info2.Name;
                    Type type2 = info2.ParameterType.IsByRef ? info2.ParameterType.GetElementType() : info2.ParameterType;
                    if (info2.Position == -1)
                    {
                        name = "(ReturnValue)";
                    }
                    object binding = null;
                    if (activity.ParameterBindings.Contains(name))
                    {
                        if (activity.ParameterBindings[name].IsBindingSet(WorkflowParameterBinding.ValueProperty))
                        {
                            binding = activity.ParameterBindings[name].GetBinding(WorkflowParameterBinding.ValueProperty);
                        }
                        else
                        {
                            binding = activity.ParameterBindings[name].GetValue(WorkflowParameterBinding.ValueProperty);
                        }
                    }
                    if (!type2.IsPublic || !type2.IsSerializable)
                    {
                        ValidationError item = new ValidationError(SR.GetString("Error_TypeNotPublicSerializable", new object[] { name, type2.FullName }), 0x567) {
                            PropertyName = (string.Compare(name, "(ReturnValue)", StringComparison.Ordinal) == 0) ? name : ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(activity2.GetType(), name)
                        };
                        errors.Add(item);
                    }
                    else if (!activity.ParameterBindings.Contains(name) || (binding == null))
                    {
                        ValidationError notSetValidationError = ValidationError.GetNotSetValidationError(name);
                        notSetValidationError.PropertyName = (string.Compare(name, "(ReturnValue)", StringComparison.Ordinal) == 0) ? name : ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(activity2.GetType(), name);
                        errors.Add(notSetValidationError);
                    }
                    else
                    {
                        AccessTypes read = AccessTypes.Read;
                        if ((info2.IsOut || info2.IsRetval) || (info2.Position == -1))
                        {
                            read = AccessTypes.Write;
                        }
                        ValidationErrorCollection errors3 = System.Workflow.Activities.Common.ValidationHelpers.ValidateProperty(manager, activity, binding, new PropertyValidationContext(activity.ParameterBindings[name], null, name), new BindValidationContext(info2.ParameterType.IsByRef ? info2.ParameterType.GetElementType() : info2.ParameterType, read));
                        foreach (ValidationError error4 in errors3)
                        {
                            if (string.Compare(name, "(ReturnValue)", StringComparison.Ordinal) != 0)
                            {
                                error4.PropertyName = ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(activity2.GetType(), name);
                            }
                        }
                        errors.AddRange(errors3);
                    }
                }
                if (activity.ParameterBindings.Count > list2.Count)
                {
                    errors.Add(new ValidationError(SR.GetString("Warning_AdditionalBindingsFound"), 0x630, true));
                }
            }
            return errors;
        }
    }
}

