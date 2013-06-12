namespace System.Diagnostics
{
    using System;
    using System.Collections;

    internal class OrdinalCaseInsensitiveComparer : IComparer
    {
        internal static readonly OrdinalCaseInsensitiveComparer Default = new OrdinalCaseInsensitiveComparer();

        public int Compare(object a, object b)
        {
            string strA = a as string;
            string strB = b as string;
            if ((strA != null) && (strB != null))
            {
                return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase);
            }
            return Comparer.Default.Compare(a, b);
        }
    }
}

