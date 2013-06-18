namespace System.Runtime.Serialization
{
    using System;
    using System.Reflection.Emit;
    using System.Runtime;

    internal class IfState
    {
        private Label elseBegin;
        private Label endIf;

        internal Label ElseBegin
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.elseBegin;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.elseBegin = value;
            }
        }

        internal Label EndIf
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.endIf;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.endIf = value;
            }
        }
    }
}

