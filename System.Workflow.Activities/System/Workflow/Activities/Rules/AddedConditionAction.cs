namespace System.Workflow.Activities.Rules
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public sealed class AddedConditionAction : RuleConditionChangeAction
    {
        private RuleCondition _conditionDefinition;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public AddedConditionAction()
        {
        }

        public AddedConditionAction(RuleCondition addedConditionDefinition)
        {
            if (addedConditionDefinition == null)
            {
                throw new ArgumentNullException("addedConditionDefinition");
            }
            this._conditionDefinition = addedConditionDefinition;
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
            if (definitions.Conditions.RuntimeMode)
            {
                definitions.Conditions.RuntimeMode = false;
                flag = true;
            }
            try
            {
                definitions.Conditions.Add(this.ConditionDefinition);
            }
            finally
            {
                if (flag)
                {
                    definitions.Conditions.RuntimeMode = true;
                }
            }
            return true;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleCondition ConditionDefinition
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._conditionDefinition;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._conditionDefinition = value;
            }
        }

        public override string ConditionName
        {
            get
            {
                return this._conditionDefinition.Name;
            }
        }
    }
}

