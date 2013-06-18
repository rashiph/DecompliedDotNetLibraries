namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public sealed class PreviousTrackingServiceAttribute : Attribute
    {
        private string assemblyQualifiedName;

        public PreviousTrackingServiceAttribute(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName))
            {
                throw new ArgumentNullException(assemblyQualifiedName);
            }
            this.assemblyQualifiedName = assemblyQualifiedName;
        }

        public string AssemblyQualifiedName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.assemblyQualifiedName;
            }
        }
    }
}

