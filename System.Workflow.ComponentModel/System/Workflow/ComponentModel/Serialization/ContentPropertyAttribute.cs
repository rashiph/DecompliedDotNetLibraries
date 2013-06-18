namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class ContentPropertyAttribute : Attribute
    {
        private string name;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ContentPropertyAttribute()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ContentPropertyAttribute(string name)
        {
            this.name = name;
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }
    }
}

