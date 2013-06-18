namespace System.Workflow.Activities.Rules
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public sealed class RemovedConditionAction : RuleConditionChangeAction
    {
        private RuleCondition _conditionDefinition;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RemovedConditionAction()
        {
        }

        public RemovedConditionAction(RuleCondition removedConditionDefinition)
        {
            if (removedConditionDefinition == null)
            {
                throw new ArgumentNullException("removedConditionDefinition");
            }
            this._conditionDefinition = removedConditionDefinition;
        }

        protected override bool ApplyTo(Activity rootActivity)
        {
            bool flag2;
            if (rootActivity == null)
            {
                return false;
            }
            RuleDefinitions definitions = rootActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if ((definitions == null) || (definitions.Conditions == null))
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
                flag2 = definitions.Conditions.Remove(this.ConditionDefinition.Name);
            }
            finally
            {
                if (flag)
                {
                    definitions.Conditions.RuntimeMode = true;
                }
            }
            return flag2;
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

