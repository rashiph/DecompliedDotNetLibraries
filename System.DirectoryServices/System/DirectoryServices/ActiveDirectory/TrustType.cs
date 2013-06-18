namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    public enum TrustType
    {
        TreeRoot,
        ParentChild,
        CrossLink,
        External,
        Forest,
        Kerberos,
        Unknown
    }
}

