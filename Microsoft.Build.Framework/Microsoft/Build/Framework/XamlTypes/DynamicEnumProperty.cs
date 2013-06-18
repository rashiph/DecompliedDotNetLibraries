namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public sealed class DynamicEnumProperty : BaseProperty
    {
        public DynamicEnumProperty()
        {
            this.ProviderSettings = new List<NameValuePair>();
        }

        public string EnumProvider
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<EnumProvider>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<EnumProvider>k__BackingField = value;
            }
        }

        public List<NameValuePair> ProviderSettings
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<ProviderSettings>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<ProviderSettings>k__BackingField = value;
            }
        }
    }
}

