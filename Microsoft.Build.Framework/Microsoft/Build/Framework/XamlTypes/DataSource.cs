namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public sealed class DataSource : ISupportInitialize
    {
        public DataSource()
        {
            this.HasConfigurationCondition = true;
            this.Label = string.Empty;
        }

        public void BeginInit()
        {
        }

        public void EndInit()
        {
        }

        public bool HasConfigurationCondition
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<HasConfigurationCondition>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<HasConfigurationCondition>k__BackingField = value;
            }
        }

        public string ItemType
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<ItemType>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<ItemType>k__BackingField = value;
            }
        }

        public string Label
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Label>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Label>k__BackingField = value;
            }
        }

        public string PersistedName
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<PersistedName>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<PersistedName>k__BackingField = value;
            }
        }

        public string Persistence
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Persistence>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Persistence>k__BackingField = value;
            }
        }

        public string SourceType
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<SourceType>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<SourceType>k__BackingField = value;
            }
        }
    }
}

