namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.Collections;
    using System.Runtime;

    internal class AutoCompletionEventArgs : EventArgs
    {
        private ICollection autoCompleteValues;
        private string prefix;

        public ICollection AutoCompleteValues
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.autoCompleteValues;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.autoCompleteValues = value;
            }
        }

        public string Prefix
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.prefix;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.prefix = value;
            }
        }
    }
}

