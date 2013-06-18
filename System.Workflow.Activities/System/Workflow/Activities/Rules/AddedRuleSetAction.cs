namespace System.Workflow.Activities.Rules
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public sealed class AddedRuleSetAction : RuleSetChangeAction
    {
        private RuleSet ruleset;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public AddedRuleSetAction()
        {
        }

        public AddedRuleSetAction(RuleSet addedRuleSetDefinition)
        {
            if (addedRuleSetDefinition == null)
            {
                throw new ArgumentNullException("addedRuleSetDefinition");
            }
            this.ruleset = addedRuleSetDefinition;
        }

        protected override bool ApplyTo(Activity rootActivity)
        {
            if (rootActivity == null)
            {
                return false;
            }
            RuleDefinitions definitions = rootActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if (definitions == null)
            {
                definitions = new RuleDefinitions();
                rootActivity.SetValue(RuleDefinitions.RuleDefinitionsProperty, definitions);
            }
            bool flag = false;
            if (definitions.RuleSets.RuntimeMode)
            {
                definitions.RuleSets.RuntimeMode = false;
                flag = true;
            }
            try
            {
                definitions.RuleSets.Add(this.ruleset);
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

