namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;

    public delegate LdapConnection QueryForConnectionCallback(LdapConnection primaryConnection, LdapConnection referralFromConnection, string newDistinguishedName, LdapDirectoryIdentifier identifier, NetworkCredential credential, long currentUserToken);
}

