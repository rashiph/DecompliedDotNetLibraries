namespace System.Workflow.Activities.Rules
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public sealed class RemovedRuleSetAction : RuleSetChangeAction
    {
        private RuleSet ruleset;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RemovedRuleSetAction()
        {
        }

        public RemovedRuleSetAction(RuleSet removedRuleSetDefinition)
        {
            if (removedRuleSetDefinition == null)
            {
                throw new ArgumentNullException("removedRuleSetDefinition");
            }
            this.ruleset = removedRuleSetDefinition;
        }

        protected override bool ApplyTo(Activity rootActivity)
        {
            bool flag2;
            if (rootActivity == null)
            {
                return false;
            }
            RuleDefinitions definitions = rootActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if ((definitions == null) || (definitions.RuleSets == null))
            {
                return false;
            }
            bool flag = false;
            if (definitions.RuleSets.RuntimeMode)
            {
                definitions.RuleSets.RuntimeMode = false;
                flag = true;
            }
            try
            {
                flag2 = definitions.RuleSets.Remove(this.ruleset.Name);
            }
            finally
            {
                if (flag)
                {
                    definitions.RuleSets.RuntimeMode = true;
                }
            }
            return flag2;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleSet RuleSetDefinition
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ruleset;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.ruleset = value;
            }
        }

        public override string RuleSetName
        {
            get
            {
                return this.ruleset.Name;
            }
        }
    }
}

