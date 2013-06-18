namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Runtime;

    internal class Token
    {
        internal int StartPosition;
        internal System.Workflow.Activities.Rules.TokenID TokenID;
        internal object Value;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal Token(System.Workflow.Activities.Rules.TokenID tokenID, int position, object value)
        {
            this.TokenID = tokenID;
            this.StartPosition = position;
            this.Value = value;
        }
    }
}

