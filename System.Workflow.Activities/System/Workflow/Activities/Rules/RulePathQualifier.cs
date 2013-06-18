namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Runtime;

    public class RulePathQualifier
    {
        private string name;
        private RulePathQualifier next;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RulePathQualifier(string name, RulePathQualifier next)
        {
            this.name = name;
            this.next = next;
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }

        public RulePathQualifier Next
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.next;
            }
        }
    }
}

