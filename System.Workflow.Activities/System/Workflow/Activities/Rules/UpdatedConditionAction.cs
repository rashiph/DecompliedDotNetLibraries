namespace System.Workflow.Activities.Rules
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public sealed class UpdatedConditionAction : RuleConditionChangeAction
    {
        private RuleCondition _conditionDefinition;
        private RuleCondition _newConditionDefinition;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UpdatedConditionAction()
        {
        }

        public UpdatedConditionAction(RuleCondition conditionDefinition, RuleCondition newConditionDefinition)
        {
            if (conditionDefinition == null)
            {
                throw new ArgumentNullException("conditionDefinition");
            }
            if (newConditionDefinition == null)
            {
                throw new ArgumentNullException("newConditionDefinition");
            }
            if (newConditionDefinition.Name != conditionDefinition.Name)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.ConditionNameNotIdentical, new object[] { newConditionDefinition.Name, conditionDefinition.Name }));
            }
            this._conditionDefinition = conditionDefinition;
            this._newConditionDefinition = newConditionDefinition;
        }

        protected override bool ApplyTo(Activity rootActivity)
        {
            if (rootActivity == null)
            {
                return false;
            }
            RuleDefinitions definitions = rootActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if ((definitions == null) || (definitions.Conditions == null))
            {
                return false;
            }
            if (definitions.Conditions[this.ConditionDefinition.Name] == null)
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
                definitions.Conditions.Remove(this.ConditionDefinition.Name);
                definitions.Conditions.Add(this.NewConditionDefinition);
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleCondition NewConditionDefinition
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._newConditionDefinition;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._newConditionDefinition = value;
            }
        }
    }
}

