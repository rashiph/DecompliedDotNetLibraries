namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    internal class RuleState : IComparable
    {
        private ICollection<int> elseActionsActiveRules;
        internal System.Workflow.Activities.Rules.Rule Rule;
        private ICollection<int> thenActionsActiveRules;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal RuleState(System.Workflow.Activities.Rules.Rule rule)
        {
            this.Rule = rule;
        }

        int IComparable.CompareTo(object obj)
        {
            RuleState state = obj as RuleState;
            int num = state.Rule.Priority.CompareTo(this.Rule.Priority);
            if (num == 0)
            {
                num = -state.Rule.Name.CompareTo(this.Rule.Name);
            }
            return num;
        }

        internal ICollection<int> ElseActionsActiveRules
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.elseActionsActiveRules;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.elseActionsActiveRules = value;
            }
        }

        internal ICollection<int> ThenActionsActiveRules
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.thenActionsActiveRules;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.thenActionsActiveRules = value;
            }
        }
    }
}

