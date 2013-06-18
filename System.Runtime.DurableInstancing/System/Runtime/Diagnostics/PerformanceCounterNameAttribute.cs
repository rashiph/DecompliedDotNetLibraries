namespace System.Runtime.Diagnostics
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Field, Inherited=false)]
    internal sealed class PerformanceCounterNameAttribute : Attribute
    {
        public PerformanceCounterNameAttribute(string name)
        {
            this.Name = name;
        }

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
    }
}

