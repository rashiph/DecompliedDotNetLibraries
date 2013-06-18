namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    internal sealed class TrustObject
    {
        public string DnsDomainName;
        public int Flags;
        public string NetbiosDomainName;
        public int OriginalIndex;
        public int ParentIndex;
        public int TrustAttributes;
        public System.DirectoryServices.ActiveDirectory.TrustType TrustType;
    }
}

