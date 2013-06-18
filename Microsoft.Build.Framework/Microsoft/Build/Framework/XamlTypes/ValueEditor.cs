namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Metadata")]
    public sealed class ValueEditor : ISupportInitialize
    {
        private string displayName;

        public ValueEditor()
        {
            this.Metadata = new List<NameValuePair>();
        }

        public void BeginInit()
        {
        }

        public void EndInit()
        {
        }

        [Localizable(true)]
        public string DisplayName
        {
            get
            {
                return (this.displayName ?? string.Empty);
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.displayName = value;
            }
        }

        public string EditorType
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<EditorType>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<EditorType>k__BackingField = value;
            }
        }

        public List<NameValuePair> Metadata
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Metadata>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Metadata>k__BackingField = value;
            }
        }
    }
}

