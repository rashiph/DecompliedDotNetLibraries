namespace System.Workflow.Activities.Rules
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public sealed class UpdatedRuleSetAction : RuleSetChangeAction
    {
        private RuleSet original;
        private RuleSet updated;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UpdatedRuleSetAction()
        {
        }

        public UpdatedRuleSetAction(RuleSet originalRuleSetDefinition, RuleSet updatedRuleSetDefinition)
        {
            if (originalRuleSetDefinition == null)
            {
                throw new ArgumentNullException("originalRuleSetDefinition");
            }
            if (updatedRuleSetDefinition == null)
            {
                throw new ArgumentNullException("updatedRuleSetDefinition");
            }
            if (originalRuleSetDefinition.Name != updatedRuleSetDefinition.Name)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.ConditionNameNotIdentical, new object[] { originalRuleSetDefinition.Name, updatedRuleSetDefinition.Name }));
            }
            this.original = originalRuleSetDefinition;
            this.updated = updatedRuleSetDefinition;
        }

        protected override bool ApplyTo(Activity rootActivity)
        {
            if (rootActivity == null)
            {
                return false;
            }
            RuleDefinitions definitions = rootActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if ((definitions == null) || (definitions.RuleSets == null))
            {
                return false;
            }
            if (definitions.RuleSets[this.RuleSetName] == null)
            {
                return false;
            }
            bool flag = false;
            if (definitions.Conditions.RuntimeMode)
            {
                definitions.Conditions.RuntimeMode = false;
                flag = true;
            }
            try
            {
                definitions.RuleSets.Remove(this.RuleSetName);
                definitions.RuleSets.Add(this.updated);
            }
            finally
            {
                if (flag)
                {
                    definitions.RuleSets.RuntimeMode = true;
                }
            }
            return true;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleSet OriginalRuleSetDefinition
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.original;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.original = value;
            }
        }

        public override string RuleSetName
        {
            get
            {
                return this.original.Name;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleSet UpdatedRuleSetDefinition
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.updated;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.updated = value;
            }
        }
    }
}

