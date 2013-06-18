namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Method, AllowMultiple=true)]
    public sealed class CorrelationAliasAttribute : Attribute
    {
        private string name;
        private string path;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CorrelationAliasAttribute(string name, string path)
        {
            this.path = path;
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

        public string Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.path;
            }
        }
    }
}

