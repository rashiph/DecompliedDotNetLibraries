namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;

    internal sealed class TypeInformation
    {
        private string assemblyString;
        private string fullTypeName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal TypeInformation(string fullTypeName, string assemblyString)
        {
            this.fullTypeName = fullTypeName;
            this.assemblyString = assemblyString;
        }

        internal string AssemblyString
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.assemblyString;
            }
        }

        internal string FullTypeName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.fullTypeName;
            }
        }
    }
}

