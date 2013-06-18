namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public sealed class StringProperty : BaseProperty
    {
        public string Subtype
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Subtype>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Subtype>k__BackingField = value;
            }
        }
    }
}

