namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct AssemblyNameReference : IComparable<AssemblyNameReference>
    {
        internal AssemblyNameExtension assemblyName;
        internal Reference reference;
        public override string ToString()
        {
            return (this.assemblyName + ", " + this.reference);
        }

        public int CompareTo(AssemblyNameReference other)
        {
            return this.assemblyName.CompareTo(other.assemblyName);
        }

        public static AssemblyNameReference Create(AssemblyNameExtension assemblyName, Reference reference)
        {
            AssemblyNameReference reference2;
            reference2.assemblyName = assemblyName;
            reference2.reference = reference;
            return reference2;
        }
    }
}

