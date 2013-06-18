namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Interface, AllowMultiple=true)]
    public sealed class CorrelationParameterAttribute : Attribute
    {
        private string name = string.Empty;

        public CorrelationParameterAttribute(string name)
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

