namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;

    public delegate bool NotifyOfNewConnectionCallback(LdapConnection primaryConnection, LdapConnection referralFromConnection, string newDistinguishedName, LdapDirectoryIdentifier identifier, LdapConnection newConnection, NetworkCredential credential, long currentUserToken, int errorCodeFromBind);
}

