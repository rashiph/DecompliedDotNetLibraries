namespace Microsoft.Build.Shared
{
    using System;
    using System.Collections.Generic;

    internal sealed class AssemblyNameReverseVersionComparer : IComparer<AssemblyNameExtension>
    {
        internal static readonly IComparer<AssemblyNameExtension> GenericComparer = new AssemblyNameReverseVersionComparer();

        public int Compare(AssemblyNameExtension x, AssemblyNameExtension y)
        {
            if ((x == null) && (y == null))
            {
                return 0;
            }
            if (y == null)
            {
                return -1;
            }
            if (x == null)
            {
                return 1;
            }
            if (!(x.Version != y.Version))
            {
                return 0;
            }
            if (y.Version == null)
            {
                return -1;
            }
            if (x.Version == null)
            {
                return 1;
            }
            return y.Version.CompareTo(x.Version);
        }
    }
}

