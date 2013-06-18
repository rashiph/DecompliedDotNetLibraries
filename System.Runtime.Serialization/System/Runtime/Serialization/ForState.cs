namespace System.Runtime.Serialization
{
    using System;
    using System.Reflection.Emit;
    using System.Runtime;

    internal class ForState
    {
        private Label beginLabel;
        private object end;
        private Label endLabel;
        private LocalBuilder indexVar;
        private bool requiresEndLabel;
        private Label testLabel;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ForState(LocalBuilder indexVar, Label beginLabel, Label testLabel, object end)
        {
            this.indexVar = indexVar;
            this.beginLabel = beginLabel;
            this.testLabel = testLabel;
            this.end = end;
        }

        internal Label BeginLabel
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.beginLabel;
            }
        }

        internal object End
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.end;
            }
        }

        internal Label EndLabel
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.endLabel;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.endLabel = value;
            }
        }

        internal LocalBuilder Index
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.indexVar;
            }
        }

        internal bool RequiresEndLabel
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.requiresEndLabel;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.requiresEndLabel = value;
            }
        }

        internal Label TestLabel
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.testLabel;
            }
        }
    }
}

