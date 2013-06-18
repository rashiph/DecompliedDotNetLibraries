namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Globalization;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class RuleConditionReferenceValidator : ConditionValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (manager.Context == null)
            {
                throw new InvalidOperationException(Messages.ContextStackMissing);
            }
            ValidationErrorCollection errors = base.Validate(manager, obj);
            RuleConditionReference reference = obj as RuleConditionReference;
            if (reference == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.UnexpectedArgumentType, new object[] { typeof(RuleConditionReference).FullName, "obj" }), "obj");
            }
            Activity activity = manager.Context[typeof(Activity)] as Activity;
            if (activity == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ContextStackItemMissing, new object[] { typeof(Activity).Name }));
            }
            if (!(manager.Context[typeof(PropertyValidationContext)] is PropertyValidationContext))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ContextStackItemMissing, new object[] { typeof(PropertyValidationContext).Name }));
            }
            if (!string.IsNullOrEmpty(reference.ConditionName))
            {
                RuleDefinitions definitions = null;
                RuleConditionCollection conditions = null;
                CompositeActivity declaringActivity = Helpers.GetDeclaringActivity(activity);
                if (declaringActivity == null)
                {
                    declaringActivity = Helpers.GetRootActivity(activity) as CompositeActivity;
                }
                if (activity.Site != null)
                {
                    definitions = ConditionHelper.Load_Rules_DT(activity.Site, declaringActivity);
                }
                else
                {
                    definitions = ConditionHelper.Load_Rules_RT(declaringActivity);
                }
                if (definitions != null)
                {
                    conditions = definitions.Conditions;
                }
                if ((conditions == null) || !conditions.Contains(reference.ConditionName))
                {
                    ValidationError error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.ConditionNotFound, new object[] { reference.ConditionName }), 0x53e) {
                        PropertyName = base.GetFullPropertyName(manager) + ".ConditionName"
                    };
                    errors.Add(error);
                    return errors;
                }
                RuleCondition condition = conditions[reference.ConditionName];
                ITypeProvider service = (ITypeProvider) manager.GetService(typeof(ITypeProvider));
                using ((WorkflowCompilationContext.Current == null) ? WorkflowCompilationContext.CreateScope(manager) : null)
                {
                    RuleValidation validation = new RuleValidation(activity, service, WorkflowCompilationContext.Current.CheckTypes);
                    condition.Validate(validation);
                    ValidationErrorCollection errors2 = validation.Errors;
                    if (errors2.Count > 0)
                    {
                        string fullPropertyName = base.GetFullPropertyName(manager);
                        string errorText = string.Format(CultureInfo.CurrentCulture, Messages.InvalidConditionExpression, new object[] { fullPropertyName });
                        int errorNumber = 0x558;
                        if (activity.Site != null)
                        {
                            ValidationError error2 = new ValidationError(errorText, errorNumber) {
                                PropertyName = fullPropertyName + ".Expression"
                            };
                            errors.Add(error2);
                        }
                        else
                        {
                            foreach (ValidationError error3 in errors2)
                            {
                                ValidationError error4 = new ValidationError(errorText + " " + error3.ErrorText, errorNumber) {
                                    PropertyName = fullPropertyName + ".Expression"
                                };
                                errors.Add(error4);
                            }
                        }
                    }
                    foreach (RuleCondition condition2 in conditions)
                    {
                        if ((condition2.Name == reference.ConditionName) && (condition2 != condition))
                        {
                            ValidationError error5 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.DuplicateConditions, new object[] { reference.ConditionName }), 0x53f) {
                                PropertyName = base.GetFullPropertyName(manager) + ".ConditionName"
                            };
                            errors.Add(error5);
                        }
                    }
                    return errors;
                }
            }
            ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.InvalidConditionName, new object[] { "ConditionName" }), 0x540) {
                PropertyName = base.GetFullPropertyName(manager) + ".ConditionName"
            };
            errors.Add(item);
            return errors;
        }
    }
}

