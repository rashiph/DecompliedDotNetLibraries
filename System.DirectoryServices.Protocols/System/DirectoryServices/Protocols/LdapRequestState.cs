namespace System.DirectoryServices.Protocols
{
    using System;

    internal class LdapRequestState
    {
        internal bool abortCalled;
        internal Exception exception;
        internal LdapAsyncResult ldapAsync;
        internal DirectoryResponse response;
    }
}

