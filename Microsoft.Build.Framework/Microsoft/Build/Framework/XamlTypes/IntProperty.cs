namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public sealed class IntProperty : BaseProperty
    {
        public override void EndInit()
        {
            base.EndInit();
        }

        public int? MaxValue
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<MaxValue>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<MaxValue>k__BackingField = value;
            }
        }

        public int? MinValue
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<MinValue>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<MinValue>k__BackingField = value;
            }
        }
    }
}

