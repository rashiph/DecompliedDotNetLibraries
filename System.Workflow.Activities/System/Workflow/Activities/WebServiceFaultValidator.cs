namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class WebServiceFaultValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            WebServiceFaultActivity activity = obj as WebServiceFaultActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(WebServiceFaultActivity).FullName }), "obj");
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
                    if (((activity3 is WebServiceFaultActivity) && (string.Compare(((WebServiceFaultActivity) activity3).InputActivityName, activity.InputActivityName, StringComparison.Ordinal) == 0)) || ((activity3 is WebServiceOutputActivity) && (string.Compare(((WebServiceOutputActivity) activity3).InputActivityName, activity.InputActivityName, StringComparison.Ordinal) == 0)))
                    {
                        if (activity3 is WebServiceFaultActivity)
                        {
                            errors.Add(new ValidationError(SR.GetString("Error_DuplicateWebServiceFaultFound", new object[] { activity3.Name, activity.InputActivityName }), 0x574));
                            return errors;
                        }
                        errors.Add(new ValidationError(SR.GetString("Error_DuplicateWebServiceResponseFound", new object[] { activity3.Name, activity.InputActivityName }), 0x56a));
                        return errors;
                    }
                }
                foreach (Activity activity4 in WebServiceActivityHelpers.GetPreceedingActivities(activity))
                {
                    if (string.Compare(activity4.QualifiedName, activity.InputActivityName, StringComparison.OrdinalIgnoreCase) == 0)
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
                WebServiceActivityHelpers.GetParameterInfo(interfaceMethod, out list, out list2);
                if (list2.Count == 0)
                {
                    errors.Add(new ValidationError(SR.GetString("Error_WebServiceFaultNotNeeded"), 0x57a));
                    return errors;
                }
            }
            return errors;
        }
    }
}

