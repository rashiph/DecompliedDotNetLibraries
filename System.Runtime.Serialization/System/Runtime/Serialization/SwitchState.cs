namespace System.Runtime.Serialization
{
    using System;
    using System.Reflection.Emit;
    using System.Runtime;

    internal class SwitchState
    {
        private bool defaultDefined;
        private Label defaultLabel;
        private Label endOfSwitchLabel;

        internal SwitchState(Label defaultLabel, Label endOfSwitchLabel)
        {
            this.defaultLabel = defaultLabel;
            this.endOfSwitchLabel = endOfSwitchLabel;
            this.defaultDefined = false;
        }

        internal bool DefaultDefined
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.defaultDefined;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.defaultDefined = value;
            }
        }

        internal Label DefaultLabel
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.defaultLabel;
            }
        }

        internal Label EndOfSwitchLabel
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.endOfSwitchLabel;
            }
        }
    }
}

