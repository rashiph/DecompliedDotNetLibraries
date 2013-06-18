namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;

    [Serializable]
    public class Rule
    {
        internal bool active;
        internal RuleReevaluationBehavior behavior;
        internal RuleCondition condition;
        internal string description;
        internal IList<RuleAction> elseActions;
        internal string name;
        internal int priority;
        private bool runtimeInitialized;
        internal IList<RuleAction> thenActions;

        public Rule()
        {
            this.behavior = RuleReevaluationBehavior.Always;
            this.active = true;
        }

        public Rule(string name)
        {
            this.behavior = RuleReevaluationBehavior.Always;
            this.active = true;
            this.name = name;
        }

        public Rule(string name, RuleCondition condition, IList<RuleAction> thenActions)
        {
            this.behavior = RuleReevaluationBehavior.Always;
            this.active = true;
            this.name = name;
            this.condition = condition;
            this.thenActions = thenActions;
        }

        public Rule(string name, RuleCondition condition, IList<RuleAction> thenActions, IList<RuleAction> elseActions)
        {
            this.behavior = RuleReevaluationBehavior.Always;
            this.active = true;
            this.name = name;
            this.condition = condition;
            this.thenActions = thenActions;
            this.elseActions = elseActions;
        }

        private static bool ActionsEqual(IList<RuleAction> myActions, IList<RuleAction> otherActions)
        {
            if ((myActions != null) || (otherActions != null))
            {
                if ((myActions == null) || (otherActions == null))
                {
                    return false;
                }
                if (myActions.Count != otherActions.Count)
                {
                    return false;
                }
                for (int i = 0; i < myActions.Count; i++)
                {
                    if (!myActions[i].Equals(otherActions[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public Rule Clone()
        {
            Rule rule = (Rule) base.MemberwiseClone();
            rule.runtimeInitialized = false;
            if (this.condition != null)
            {
                rule.condition = this.condition.Clone();
            }
            if (this.thenActions != null)
            {
                rule.thenActions = new List<RuleAction>();
                foreach (RuleAction action in this.thenActions)
                {
                    rule.thenActions.Add(action.Clone());
                }
            }
            if (this.elseActions != null)
            {
                rule.elseActions = new List<RuleAction>();
                foreach (RuleAction action2 in this.elseActions)
                {
                    rule.elseActions.Add(action2.Clone());
                }
            }
            return rule;
        }

        public override bool Equals(object obj)
        {
            Rule rule = obj as Rule;
            if (rule == null)
            {
                return false;
            }
            if (((this.Name != rule.Name) || (this.Description != rule.Description)) || (((this.Active != rule.Active) || (this.ReevaluationBehavior != rule.ReevaluationBehavior)) || (this.Priority != rule.Priority)))
            {
                return false;
            }
            if (this.Condition == null)
            {
                if (rule.Condition != null)
                {
                    return false;
                }
            }
            else if (!this.Condition.Equals(rule.Condition))
            {
                return false;
            }
            if (!ActionsEqual(this.thenActions, rule.thenActions))
            {
                return false;
            }
            if (!ActionsEqual(this.elseActions, rule.elseActions))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal void OnRuntimeInitialized()
        {
            this.runtimeInitialized = true;
        }

        internal void Validate(RuleValidation validation)
        {
            int count = validation.Errors.Count;
            if (string.IsNullOrEmpty(this.name))
            {
                validation.Errors.Add(new ValidationError(Messages.RuleNameMissing, 0x540));
            }
            if (this.condition == null)
            {
                validation.Errors.Add(new ValidationError(Messages.MissingRuleCondition, 0x57d));
            }
            else
            {
                this.condition.Validate(validation);
            }
            if (this.thenActions != null)
            {
                ValidateRuleActions(this.thenActions, validation);
            }
            if (this.elseActions != null)
            {
                ValidateRuleActions(this.elseActions, validation);
            }
            ValidationErrorCollection errors = validation.Errors;
            if (errors.Count > count)
            {
                string str = string.Format(CultureInfo.CurrentCulture, Messages.RuleValidationError, new object[] { this.name });
                int num2 = errors.Count;
                for (int i = count; i < num2; i++)
                {
                    ValidationError error = errors[i];
                    ValidationError error2 = new ValidationError(str + error.ErrorText, error.ErrorNumber, error.IsWarning);
                    foreach (DictionaryEntry entry in error.UserData)
                    {
                        error2.UserData[entry.Key] = entry.Value;
                    }
                    errors[i] = error2;
                }
            }
        }

        private static void ValidateRuleActions(ICollection<RuleAction> ruleActions, RuleValidation validator)
        {
            bool flag = false;
            bool flag2 = false;
            foreach (RuleAction action in ruleActions)
            {
                action.Validate(validator);
                if (flag)
                {
                    flag2 = true;
                }
                if (action is RuleHaltAction)
                {
                    flag = true;
                }
            }
            if (flag2)
            {
                validator.Errors.Add(new ValidationError(Messages.UnreachableCodeHalt, 0x54c, true));
            }
        }

        public bool Active
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.active;
            }
            set
            {
                if (this.runtimeInitialized)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                this.active = value;
            }
        }

        public RuleCondition Condition
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.condition;
            }
            set
            {
                if (this.runtimeInitialized)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                this.condition = value;
            }
        }

        public string Description
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.description;
            }
            set
            {
                if (this.runtimeInitialized)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                this.description = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IList<RuleAction> ElseActions
        {
            get
            {
                if (this.elseActions == null)
                {
                    this.elseActions = new List<RuleAction>();
                }
                return this.elseActions;
            }
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
            set
            {
                if (this.runtimeInitialized)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                this.name = value;
            }
        }

        public int Priority
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.priority;
            }
            set
            {
                if (this.runtimeInitialized)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                this.priority = value;
            }
        }

        public RuleReevaluationBehavior ReevaluationBehavior
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.behavior;
            }
            set
            {
                if (this.runtimeInitialized)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                this.behavior = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IList<RuleAction> ThenActions
        {
            get
            {
                if (this.thenActions == null)
                {
                    this.thenActions = new List<RuleAction>();
                }
                return this.thenActions;
            }
        }
    }
}

