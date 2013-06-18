namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void DereferenceConnectionCallback(LdapConnection primaryConnection, LdapConnection connectionToDereference);
}

