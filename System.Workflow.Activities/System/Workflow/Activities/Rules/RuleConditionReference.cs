namespace System.Workflow.Activities.Rules
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Workflow.Activities.Common;
    using System.Workflow.Activities.Rules.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    [SRDisplayName("RuleConditionDisplayName"), TypeConverter(typeof(RuleConditionReferenceTypeConverter)), ActivityValidator(typeof(RuleConditionReferenceValidator))]
    public class RuleConditionReference : ActivityCondition
    {
        private string _condition;
        private bool _runtimeInitialized;
        private string declaringActivityId = string.Empty;
        [NonSerialized]
        private object syncLock = new object();

        public override bool Evaluate(Activity activity, IServiceProvider provider)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (string.IsNullOrEmpty(this._condition))
            {
                throw new InvalidOperationException(SR.GetString("Error_MissingConditionName", new object[] { activity.Name }));
            }
            RuleDefinitions ruleDefinitions = null;
            if (string.IsNullOrEmpty(this.declaringActivityId))
            {
                CompositeActivity declaringActivity = null;
                ruleDefinitions = GetRuleDefinitions(activity, out declaringActivity);
            }
            else
            {
                ruleDefinitions = (RuleDefinitions) activity.GetActivityByName(this.declaringActivityId).GetValue(RuleDefinitions.RuleDefinitionsProperty);
            }
            if ((ruleDefinitions == null) || (ruleDefinitions.Conditions == null))
            {
                throw new InvalidOperationException(SR.GetString("Error_MissingRuleConditions"));
            }
            RuleCondition condition = ruleDefinitions.Conditions[this._condition];
            if (condition == null)
            {
                return true;
            }
            Activity enclosingActivity = Helpers.GetEnclosingActivity(activity);
            RuleValidation validation = new RuleValidation(enclosingActivity);
            if (!condition.Validate(validation))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ConditionValidationFailed, new object[] { this._condition }));
            }
            RuleExecution execution = new RuleExecution(validation, enclosingActivity, provider as ActivityExecutionContext);
            return condition.Evaluate(execution);
        }

        private static RuleDefinitions GetRuleDefinitions(Activity activity, out CompositeActivity declaringActivity)
        {
            declaringActivity = Helpers.GetDeclaringActivity(activity);
            if (declaringActivity == null)
            {
                declaringActivity = Helpers.GetRootActivity(activity) as CompositeActivity;
            }
            return ConditionHelper.Load_Rules_RT(declaringActivity);
        }

        protected override void InitializeProperties()
        {
            lock (this.syncLock)
            {
                if (!this._runtimeInitialized)
                {
                    CompositeActivity declaringActivity = null;
                    Activity parentDependencyObject = base.ParentDependencyObject as Activity;
                    GetRuleDefinitions(parentDependencyObject, out declaringActivity).OnRuntimeInitialized();
                    this.declaringActivityId = declaringActivity.QualifiedName;
                    base.InitializeProperties();
                    this._runtimeInitialized = true;
                }
            }
        }

        public string ConditionName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._condition;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._condition = value;
            }
        }
    }
}

