namespace Microsoft.Build.Framework
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class TaskPropertyInfo
    {
        public TaskPropertyInfo(string name, Type typeOfParameter, bool output, bool required)
        {
            this.Name = name;
            this.PropertyType = typeOfParameter;
            this.Output = output;
            this.Required = required;
        }

        public string Name
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Name>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<Name>k__BackingField = value;
            }
        }

        public bool Output
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Output>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<Output>k__BackingField = value;
            }
        }

        public Type PropertyType
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<PropertyType>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<PropertyType>k__BackingField = value;
            }
        }

        public bool Required
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Required>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<Required>k__BackingField = value;
            }
        }
    }
}

