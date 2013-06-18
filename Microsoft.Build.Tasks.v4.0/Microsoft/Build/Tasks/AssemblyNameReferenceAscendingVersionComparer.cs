namespace Microsoft.Build.Tasks
{
    using System;
    using System.Collections;

    internal sealed class AssemblyNameReferenceAscendingVersionComparer : IComparer
    {
        internal static readonly IComparer comparer = new AssemblyNameReferenceAscendingVersionComparer();

        private AssemblyNameReferenceAscendingVersionComparer()
        {
        }

        public int Compare(object o1, object o2)
        {
            AssemblyNameReference reference = (AssemblyNameReference) o1;
            AssemblyNameReference reference2 = (AssemblyNameReference) o2;
            Version version = reference.assemblyName.Version;
            Version version2 = reference2.assemblyName.Version;
            if (version == null)
            {
                version = new Version();
            }
            if (version2 == null)
            {
                version2 = new Version();
            }
            return version.CompareTo(version2);
        }
    }
}

