namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    internal class ADFilter
    {
        public FilterContent Filter = new FilterContent();
        public FilterType Type;

        [StructLayout(LayoutKind.Sequential)]
        public struct FilterContent
        {
            public ArrayList And;
            public ArrayList Or;
            public ADFilter Not;
            public ADAttribute EqualityMatch;
            public ADSubstringFilter Substrings;
            public ADAttribute GreaterOrEqual;
            public ADAttribute LessOrEqual;
            public string Present;
            public ADAttribute ApproxMatch;
            public ADExtenMatchFilter ExtensibleMatch;
        }

        public enum FilterType
        {
            And,
            Or,
            Not,
            EqualityMatch,
            Substrings,
            GreaterOrEqual,
            LessOrEqual,
            Present,
            ApproxMatch,
            ExtensibleMatch
        }
    }
}

