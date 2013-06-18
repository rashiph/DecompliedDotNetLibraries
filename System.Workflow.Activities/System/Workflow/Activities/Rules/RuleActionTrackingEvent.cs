namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Runtime;

    [Serializable]
    public class RuleActionTrackingEvent
    {
        private bool conditionResult;
        private string ruleName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal RuleActionTrackingEvent(string ruleName, bool conditionResult)
        {
            this.ruleName = ruleName;
            this.conditionResult = conditionResult;
        }

        public bool ConditionResult
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.conditionResult;
            }
        }

        public string RuleName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ruleName;
            }
        }
    }
}

