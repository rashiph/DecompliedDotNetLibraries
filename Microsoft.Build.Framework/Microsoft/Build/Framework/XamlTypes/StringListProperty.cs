namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public sealed class StringListProperty : BaseProperty
    {
        public StringListProperty()
        {
            this.RendererValueSeparator = ";";
        }

        public string CommandLineValueSeparator
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<CommandLineValueSeparator>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<CommandLineValueSeparator>k__BackingField = value;
            }
        }

        public string RendererValueSeparator
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<RendererValueSeparator>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<RendererValueSeparator>k__BackingField = value;
            }
        }

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

