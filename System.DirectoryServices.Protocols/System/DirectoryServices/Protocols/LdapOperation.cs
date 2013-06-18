namespace System.DirectoryServices.Protocols
{
    using System;

    internal enum LdapOperation
    {
        LdapAdd,
        LdapModify,
        LdapSearch,
        LdapDelete,
        LdapModifyDn,
        LdapCompare,
        LdapExtendedRequest
    }
}

