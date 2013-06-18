namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Runtime;

    internal class IntellisenseKeyword
    {
        private string name;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal IntellisenseKeyword(string name)
        {
            this.name = name;
        }

        internal string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }
    }
}

