namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate DirectoryResponse GetLdapResponseCallback(int messageId, LdapOperation operation, ResultAll resultType, TimeSpan requestTimeout, bool exceptionOnTimeOut);
}

