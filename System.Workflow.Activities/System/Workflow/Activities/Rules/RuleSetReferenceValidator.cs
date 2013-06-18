namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Globalization;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class RuleSetReferenceValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            RuleSetReference reference = obj as RuleSetReference;
            if (reference == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.UnexpectedArgumentType, new object[] { typeof(RuleSetReference).FullName, "obj" }), "obj");
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
            if (!string.IsNullOrEmpty(reference.RuleSetName))
            {
                RuleDefinitions definitions = null;
                RuleSetCollection ruleSets = null;
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
                    ruleSets = definitions.RuleSets;
                }
                if ((ruleSets == null) || !ruleSets.Contains(reference.RuleSetName))
                {
                    ValidationError error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.RuleSetNotFound, new object[] { reference.RuleSetName }), 0x576) {
                        PropertyName = base.GetFullPropertyName(manager) + ".RuleSetName"
                    };
                    errors.Add(error);
                    return errors;
                }
                RuleSet set = ruleSets[reference.RuleSetName];
                ITypeProvider service = (ITypeProvider) manager.GetService(typeof(ITypeProvider));
                using ((WorkflowCompilationContext.Current == null) ? WorkflowCompilationContext.CreateScope(manager) : null)
                {
                    RuleValidation validation = new RuleValidation(activity, service, WorkflowCompilationContext.Current.CheckTypes);
                    set.Validate(validation);
                    ValidationErrorCollection errors2 = validation.Errors;
                    if (errors2.Count > 0)
                    {
                        string fullPropertyName = base.GetFullPropertyName(manager);
                        string str6 = string.Format(CultureInfo.CurrentCulture, Messages.InvalidRuleSetExpression, new object[] { fullPropertyName });
                        int errorNumber = 0x577;
                        if (activity.Site != null)
                        {
                            foreach (ValidationError error2 in errors2)
                            {
                                ValidationError error3 = new ValidationError(error2.ErrorText, errorNumber) {
                                    PropertyName = fullPropertyName + ".RuleSet Definition"
                                };
                                errors.Add(error3);
                            }
                            return errors;
                        }
                        foreach (ValidationError error4 in errors2)
                        {
                            ValidationError error5 = new ValidationError(str6 + " " + error4.ErrorText, errorNumber) {
                                PropertyName = fullPropertyName
                            };
                            errors.Add(error5);
                        }
                    }
                    return errors;
                }
            }
            ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.InvalidRuleSetName, new object[] { "RuleSetReference" }), 0x578) {
                PropertyName = base.GetFullPropertyName(manager) + ".RuleSetName"
            };
            errors.Add(item);
            return errors;
        }
    }
}

