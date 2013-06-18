namespace Microsoft.Build.Framework
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public sealed class RequiredRuntimeAttribute : Attribute
    {
        private string runtimeVersion;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RequiredRuntimeAttribute(string runtimeVersion)
        {
            this.runtimeVersion = runtimeVersion;
        }

        public string RuntimeVersion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.runtimeVersion;
            }
        }
    }
}

