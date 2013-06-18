namespace System.DirectoryServices.Protocols
{
    using System;

    internal class ADExtenMatchFilter
    {
        public bool DNAttributes = false;
        public string MatchingRule;
        public string Name;
        public ADValue Value = null;
    }
}

