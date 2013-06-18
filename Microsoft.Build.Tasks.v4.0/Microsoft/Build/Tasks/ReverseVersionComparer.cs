namespace Microsoft.Build.Tasks
{
    using System;
    using System.Collections;

    internal sealed class ReverseVersionComparer : IComparer
    {
        internal static readonly ReverseVersionComparer Comparer = new ReverseVersionComparer();
        private IComparer forwardCompare = StringComparer.Ordinal;

        int IComparer.Compare(object x, object y)
        {
            return this.forwardCompare.Compare(y, x);
        }
    }
}

