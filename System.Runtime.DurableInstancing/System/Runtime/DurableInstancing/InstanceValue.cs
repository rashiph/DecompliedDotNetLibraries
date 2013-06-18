namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public sealed class InstanceValue
    {
        private static readonly InstanceValue deletedValue = new InstanceValue();

        private InstanceValue()
        {
            this.Value = this;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceValue(object value) : this(value, InstanceValueOptions.None)
        {
        }

        public InstanceValue(object value, InstanceValueOptions options)
        {
            this.Value = value;
            this.Options = options;
        }

        public static InstanceValue DeletedValue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return deletedValue;
            }
        }

        public bool IsDeletedValue
        {
            get
            {
                return object.ReferenceEquals(this, DeletedValue);
            }
        }

        public InstanceValueOptions Options
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Options>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<Options>k__BackingField = value;
            }
        }

        public object Value
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Value>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<Value>k__BackingField = value;
            }
        }
    }
}

