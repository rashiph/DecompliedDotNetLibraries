namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public sealed class Argument : ISupportInitialize
    {
        public Argument()
        {
            this.Separator = string.Empty;
        }

        public void BeginInit()
        {
        }

        public void EndInit()
        {
        }

        public bool IsRequired
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<IsRequired>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<IsRequired>k__BackingField = value;
            }
        }

        public string Property
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Property>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Property>k__BackingField = value;
            }
        }

        public string Separator
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Separator>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Separator>k__BackingField = value;
            }
        }
    }
}

