namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class WebServiceReceiveValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            WebServiceInputActivity activity = obj as WebServiceInputActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(WebServiceInputActivity).FullName }), "obj");
            }
            if (!Helpers.IsActivityLocked(activity))
            {
                List<ParameterInfo> list;
                List<ParameterInfo> list2;
                if (activity.IsActivating)
                {
                    if (WebServiceActivityHelpers.GetPreceedingActivities(activity).GetEnumerator().MoveNext())
                    {
                        ValidationError item = new ValidationError(SR.GetString("Error_ActivationActivityNotFirst"), 0x568) {
                            PropertyName = "IsActivating"
                        };
                        errors.Add(item);
                        return errors;
                    }
                    if (WebServiceActivityHelpers.IsInsideLoop(activity, null))
                    {
                        ValidationError error2 = new ValidationError(SR.GetString("Error_ActivationActivityInsideLoop"), 0x579) {
                            PropertyName = "IsActivating"
                        };
                        errors.Add(error2);
                        return errors;
                    }
                }
                else if (!WebServiceActivityHelpers.GetPreceedingActivities(activity, true).GetEnumerator().MoveNext())
                {
                    ValidationError error3 = new ValidationError(SR.GetString("Error_WebServiceReceiveNotMarkedActivate"), 0x569) {
                        PropertyName = "IsActivating"
                    };
                    errors.Add(error3);
                    return errors;
                }
                ITypeProvider service = (ITypeProvider) manager.GetService(typeof(ITypeProvider));
                if (service == null)
                {
                    throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
                }
                Type interfaceType = null;
                if (activity.InterfaceType != null)
                {
                    interfaceType = service.GetType(activity.InterfaceType.AssemblyQualifiedName);
                }
                if (interfaceType == null)
                {
                    ValidationError error4 = new ValidationError(SR.GetString("Error_TypePropertyInvalid", new object[] { "InterfaceType" }), 0x116) {
                        PropertyName = "InterfaceType"
                    };
                    errors.Add(error4);
                    return errors;
                }
                if (!interfaceType.IsInterface)
                {
                    ValidationError error5 = new ValidationError(SR.GetString("Error_InterfaceTypeNotInterface", new object[] { "InterfaceType" }), 0x570) {
                        PropertyName = "InterfaceType"
                    };
                    errors.Add(error5);
                    return errors;
                }
                if (string.IsNullOrEmpty(activity.MethodName))
                {
                    errors.Add(ValidationError.GetNotSetValidationError("MethodName"));
                    return errors;
                }
                MethodInfo interfaceMethod = Helpers.GetInterfaceMethod(interfaceType, activity.MethodName);
                if (interfaceMethod == null)
                {
                    ValidationError error6 = new ValidationError(SR.GetString("Error_MethodNotExists", new object[] { "MethodName", activity.MethodName }), 0x137) {
                        PropertyName = "MethodName"
                    };
                    errors.Add(error6);
                    return errors;
                }
                ValidationErrorCollection errors2 = WebServiceActivityHelpers.ValidateParameterTypes(interfaceMethod);
                if (errors2.Count > 0)
                {
                    foreach (ValidationError error7 in errors2)
                    {
                        error7.PropertyName = "MethodName";
                    }
                    errors.AddRange(errors2);
                    return errors;
                }
                WebServiceActivityHelpers.GetParameterInfo(interfaceMethod, out list, out list2);
                foreach (ParameterInfo info2 in list)
                {
                    string name = info2.Name;
                    string parameterPropertyName = ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(activity.GetType(), name);
                    Type type2 = info2.ParameterType.IsByRef ? info2.ParameterType.GetElementType() : info2.ParameterType;
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
                        ValidationError error8 = new ValidationError(SR.GetString("Error_TypeNotPublicSerializable", new object[] { name, type2.FullName }), 0x567) {
                            PropertyName = parameterPropertyName
                        };
                        errors.Add(error8);
                    }
                    else if (!activity.ParameterBindings.Contains(name) || (binding == null))
                    {
                        ValidationError notSetValidationError = ValidationError.GetNotSetValidationError(name);
                        notSetValidationError.PropertyName = parameterPropertyName;
                        errors.Add(notSetValidationError);
                    }
                    else
                    {
                        AccessTypes read = AccessTypes.Read;
                        if (info2.ParameterType.IsByRef)
                        {
                            read |= AccessTypes.Write;
                        }
                        ValidationErrorCollection errors3 = System.Workflow.Activities.Common.ValidationHelpers.ValidateProperty(manager, activity, binding, new PropertyValidationContext(activity.ParameterBindings[name], null, name), new BindValidationContext(info2.ParameterType.IsByRef ? info2.ParameterType.GetElementType() : info2.ParameterType, read));
                        foreach (ValidationError error10 in errors3)
                        {
                            error10.PropertyName = parameterPropertyName;
                        }
                        errors.AddRange(errors3);
                    }
                }
                if (activity.ParameterBindings.Count > list.Count)
                {
                    errors.Add(new ValidationError(SR.GetString("Warning_AdditionalBindingsFound"), 0x630, true));
                }
                bool flag = false;
                foreach (Activity activity2 in WebServiceActivityHelpers.GetSucceedingActivities(activity))
                {
                    if (((activity2 is WebServiceOutputActivity) && (((WebServiceOutputActivity) activity2).InputActivityName == activity.Name)) || ((activity2 is WebServiceFaultActivity) && (((WebServiceFaultActivity) activity2).InputActivityName == activity.Name)))
                    {
                        flag = true;
                        break;
                    }
                }
                if (((list2.Count > 0) || (interfaceMethod.ReturnType != typeof(void))) && !flag)
                {
                    errors.Add(new ValidationError(SR.GetString("Error_WebServiceResponseNotFound"), 0x55d));
                }
            }
            return errors;
        }
    }
}

