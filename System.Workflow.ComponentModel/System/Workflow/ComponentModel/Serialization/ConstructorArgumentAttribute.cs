namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=false)]
    public sealed class ConstructorArgumentAttribute : Attribute
    {
        private string argumentName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConstructorArgumentAttribute(string argumentName)
        {
            this.argumentName = argumentName;
        }

        public string ArgumentName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.argumentName;
            }
        }
    }
}

