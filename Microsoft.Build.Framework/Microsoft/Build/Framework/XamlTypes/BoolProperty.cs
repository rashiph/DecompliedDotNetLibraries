namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public sealed class BoolProperty : BaseProperty
    {
        public string ReverseSwitch
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<ReverseSwitch>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<ReverseSwitch>k__BackingField = value;
            }
        }
    }
}

