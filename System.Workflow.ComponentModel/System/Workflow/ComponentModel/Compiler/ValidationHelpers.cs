namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    internal static class ValidationHelpers
    {
        internal static bool IsActivitySourceInOrder(Activity request, Activity response)
        {
            if (request.Parent == null)
            {
                return true;
            }
            List<Activity> list = new List<Activity> {
                response
            };
            for (Activity activity = (response is CompositeActivity) ? ((CompositeActivity) response) : response.Parent; activity != null; activity = activity.Parent)
            {
                list.Add(activity);
            }
            Activity activity2 = request;
            CompositeActivity item = (request is CompositeActivity) ? ((CompositeActivity) request) : request.Parent;
            while ((item != null) && !list.Contains(item))
            {
                activity2 = item;
                item = item.Parent;
            }
            if (item == activity2)
            {
                return true;
            }
            bool flag = false;
            int num = list.IndexOf(item) - 1;
            num = (num < 0) ? 0 : num;
            Activity activity4 = list[num];
            if (((item == null) || Helpers.IsAlternateFlowActivity(activity2)) || Helpers.IsAlternateFlowActivity(activity4))
            {
                flag = true;
            }
            else
            {
                for (int i = 0; i < item.EnabledActivities.Count; i++)
                {
                    if (item.EnabledActivities[i] == activity2)
                    {
                        break;
                    }
                    if (item.EnabledActivities[i] == activity4)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            return !flag;
        }

        internal static ValidationErrorCollection ValidateActivity(ValidationManager manager, Activity activity)
        {
            ValidationErrorCollection errors = ValidateObject(manager, activity);
            foreach (ValidationError error in errors)
            {
                if (!error.UserData.Contains(typeof(Activity)))
                {
                    error.UserData[typeof(Activity)] = activity;
                }
            }
            return errors;
        }

        internal static void ValidateIdentifier(IServiceProvider serviceProvider, string identifier)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            SupportedLanguages supportedLanguage = CompilerHelpers.GetSupportedLanguage(serviceProvider);
            CodeDomProvider codeDomProvider = CompilerHelpers.GetCodeDomProvider(supportedLanguage);
            if ((((supportedLanguage == SupportedLanguages.CSharp) && identifier.StartsWith("@", StringComparison.Ordinal)) || (((supportedLanguage == SupportedLanguages.VB) && identifier.StartsWith("[", StringComparison.Ordinal)) && identifier.EndsWith("]", StringComparison.Ordinal))) || !codeDomProvider.IsValidIdentifier(identifier))
            {
                throw new Exception(SR.GetString("Error_InvalidLanguageIdentifier", new object[] { identifier }));
            }
        }

        internal static ValidationError ValidateIdentifier(string propName, IServiceProvider context, string identifier)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            ValidationError error = null;
            if ((identifier == null) || (identifier.Length == 0))
            {
                error = new ValidationError(SR.GetString("Error_PropertyNotSet", new object[] { propName }), 0x116);
            }
            else
            {
                try
                {
                    ValidateIdentifier(context, identifier);
                }
                catch (Exception exception)
                {
                    error = new ValidationError(SR.GetString("Error_InvalidIdentifier", new object[] { propName, exception.Message }), 0x119);
                }
            }
            if (error != null)
            {
                error.PropertyName = propName;
            }
            return error;
        }

        internal static ValidationError ValidateNameProperty(string propName, IServiceProvider context, string identifier)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            ValidationError error = null;
            if ((identifier == null) || (identifier.Length == 0))
            {
                error = new ValidationError(SR.GetString("Error_PropertyNotSet", new object[] { propName }), 0x116);
            }
            else
            {
                SupportedLanguages supportedLanguage = CompilerHelpers.GetSupportedLanguage(context);
                CodeDomProvider codeDomProvider = CompilerHelpers.GetCodeDomProvider(supportedLanguage);
                if ((((supportedLanguage == SupportedLanguages.CSharp) && identifier.StartsWith("@", StringComparison.Ordinal)) || (((supportedLanguage == SupportedLanguages.VB) && identifier.StartsWith("[", StringComparison.Ordinal)) && identifier.EndsWith("]", StringComparison.Ordinal))) || !codeDomProvider.IsValidIdentifier(codeDomProvider.CreateEscapedIdentifier(identifier)))
                {
                    error = new ValidationError(SR.GetString("Error_InvalidIdentifier", new object[] { propName, SR.GetString("Error_InvalidLanguageIdentifier", new object[] { identifier }) }), 0x119);
                }
            }
            if (error != null)
            {
                error.PropertyName = propName;
            }
            return error;
        }

        internal static ValidationErrorCollection ValidateObject(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection();
            if (obj != null)
            {
                Type type = obj.GetType();
                if (type.IsPrimitive || !(type != typeof(string)))
                {
                    return errors;
                }
                bool flag = false;
                Dictionary<int, object> context = manager.Context[typeof(Dictionary<int, object>)] as Dictionary<int, object>;
                if (context == null)
                {
                    context = new Dictionary<int, object>();
                    manager.Context.Push(context);
                    flag = true;
                }
                try
                {
                    if (context.ContainsKey(obj.GetHashCode()))
                    {
                        return errors;
                    }
                    context.Add(obj.GetHashCode(), obj);
                    try
                    {
                        foreach (Validator validator in manager.GetValidators(type))
                        {
                            errors.AddRange(validator.Validate(manager, obj));
                        }
                    }
                    finally
                    {
                        context.Remove(obj.GetHashCode());
                    }
                }
                finally
                {
                    if (flag)
                    {
                        manager.Context.Pop();
                    }
                }
            }
            return errors;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static ValidationErrorCollection ValidateProperty(ValidationManager manager, Activity activity, object obj, PropertyValidationContext propertyValidationContext)
        {
            return ValidateProperty(manager, activity, obj, propertyValidationContext, null);
        }

        internal static ValidationErrorCollection ValidateProperty(ValidationManager manager, Activity activity, object obj, PropertyValidationContext propertyValidationContext, object extendedPropertyContext)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (propertyValidationContext == null)
            {
                throw new ArgumentNullException("propertyValidationContext");
            }
            ValidationErrorCollection errors = new ValidationErrorCollection();
            manager.Context.Push(activity);
            manager.Context.Push(propertyValidationContext);
            if (extendedPropertyContext != null)
            {
                manager.Context.Push(extendedPropertyContext);
            }
            try
            {
                errors.AddRange(ValidateObject(manager, obj));
            }
            finally
            {
                manager.Context.Pop();
                manager.Context.Pop();
                if (extendedPropertyContext != null)
                {
                    manager.Context.Pop();
                }
            }
            return errors;
        }

        internal static ValidationErrorCollection ValidateUniqueIdentifiers(Activity rootActivity)
        {
            if (rootActivity == null)
            {
                throw new ArgumentNullException("rootActivity");
            }
            Hashtable hashtable = new Hashtable();
            ValidationErrorCollection errors = new ValidationErrorCollection();
            Queue queue = new Queue();
            queue.Enqueue(rootActivity);
            while (queue.Count > 0)
            {
                Activity activity = (Activity) queue.Dequeue();
                if (activity.Enabled)
                {
                    if (hashtable.ContainsKey(activity.QualifiedName))
                    {
                        ValidationError item = new ValidationError(SR.GetString("Error_DuplicatedActivityID", new object[] { activity.QualifiedName }), 0x602) {
                            PropertyName = "Name"
                        };
                        item.UserData[typeof(Activity)] = activity;
                        errors.Add(item);
                    }
                    else
                    {
                        hashtable.Add(activity.QualifiedName, activity);
                    }
                    if ((activity is CompositeActivity) && ((activity.Parent == null) || !Helpers.IsCustomActivity(activity as CompositeActivity)))
                    {
                        foreach (Activity activity2 in Helpers.GetAllEnabledActivities((CompositeActivity) activity))
                        {
                            queue.Enqueue(activity2);
                        }
                    }
                }
            }
            return errors;
        }
    }
}

