namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;

    internal sealed class TypeInformation
    {
        private string assemblyString;
        private string fullTypeName;
        private bool hasTypeForwardedFrom;

        internal TypeInformation(string fullTypeName, string assemblyString, bool hasTypeForwardedFrom)
        {
            this.fullTypeName = fullTypeName;
            this.assemblyString = assemblyString;
            this.hasTypeForwardedFrom = hasTypeForwardedFrom;
        }

        internal string AssemblyString
        {
            get
            {
                return this.assemblyString;
            }
        }

        internal string FullTypeName
        {
            get
            {
                return this.fullTypeName;
            }
        }

        internal bool HasTypeForwardedFrom
        {
            get
            {
                return this.hasTypeForwardedFrom;
            }
        }
    }
}

