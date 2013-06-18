namespace System.Workflow.ComponentModel
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple=false)]
    internal sealed class ActivityExecutorAttribute : Attribute
    {
        private string executorTypeName;

        public ActivityExecutorAttribute(string executorTypeName)
        {
            this.executorTypeName = string.Empty;
            this.executorTypeName = executorTypeName;
        }

        public ActivityExecutorAttribute(Type executorType)
        {
            this.executorTypeName = string.Empty;
            if (executorType != null)
            {
                this.executorTypeName = executorType.AssemblyQualifiedName;
            }
        }

        public string ExecutorTypeName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.executorTypeName;
            }
        }
    }
}

