namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public class NameValuePair
    {
        public string Name
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Name>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Name>k__BackingField = value;
            }
        }

        [Localizable(true)]
        public string Value
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Value>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Value>k__BackingField = value;
            }
        }
    }
}

