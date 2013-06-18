namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class InvokeWebServiceValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            InvokeWebServiceActivity activity = obj as InvokeWebServiceActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(InvokeWebServiceActivity).FullName }), "obj");
            }
            if (activity.ProxyClass == null)
            {
                ValidationError item = new ValidationError(SR.GetString("Error_TypePropertyInvalid", new object[] { "ProxyClass" }), 0x116) {
                    PropertyName = "ProxyClass"
                };
                errors.Add(item);
                return errors;
            }
            if (((ITypeProvider) manager.GetService(typeof(ITypeProvider))) == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
            }
            Type proxyClass = activity.ProxyClass;
            if ((activity.MethodName == null) || (activity.MethodName.Length == 0))
            {
                errors.Add(ValidationError.GetNotSetValidationError("MethodName"));
                return errors;
            }
            MethodInfo method = proxyClass.GetMethod(activity.MethodName);
            if (method == null)
            {
                ValidationError error2 = new ValidationError(SR.GetString("Error_MethodNotExists", new object[] { "MethodName", activity.MethodName }), 0x137) {
                    PropertyName = "MethodName"
                };
                errors.Add(error2);
                return errors;
            }
            ArrayList list = new ArrayList(method.GetParameters());
            if (method.ReturnType != typeof(void))
            {
                list.Add(method.ReturnParameter);
            }
            foreach (ParameterInfo info2 in list)
            {
                string name = info2.Name;
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
                if (!activity.ParameterBindings.Contains(name) || (binding == null))
                {
                    ValidationError notSetValidationError = ValidationError.GetNotSetValidationError(name);
                    if (InvokeWebServiceActivity.ReservedParameterNames.Contains(name))
                    {
                        notSetValidationError.PropertyName = ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(activity.GetType(), name);
                    }
                    notSetValidationError.PropertyName = name;
                    errors.Add(notSetValidationError);
                }
                else
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
                    ValidationErrorCollection errors2 = System.Workflow.Activities.Common.ValidationHelpers.ValidateProperty(manager, activity, binding, new PropertyValidationContext(activity.ParameterBindings[name], null, name), new BindValidationContext(info2.ParameterType.IsByRef ? info2.ParameterType.GetElementType() : info2.ParameterType, read));
                    if (InvokeWebServiceActivity.ReservedParameterNames.Contains(name))
                    {
                        foreach (ValidationError error4 in errors2)
                        {
                            error4.PropertyName = ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(activity.GetType(), name);
                        }
                    }
                    errors.AddRange(errors2);
                }
            }
            if (activity.ParameterBindings.Count > list.Count)
            {
                errors.Add(new ValidationError(SR.GetString("Warning_AdditionalBindingsFound"), 0x630, true));
            }
            return errors;
        }
    }
}

