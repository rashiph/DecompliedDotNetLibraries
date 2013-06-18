namespace Microsoft.Build.Tasks
{
    using System;
    using System.Collections.Generic;

    internal sealed class ReverseVersionGenericComparer : IComparer<Version>
    {
        internal static readonly ReverseVersionGenericComparer Comparer = new ReverseVersionGenericComparer();

        int IComparer<Version>.Compare(Version x, Version y)
        {
            return y.CompareTo(x);
        }
    }
}

