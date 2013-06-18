namespace System.DirectoryServices.Protocols
{
    using System;

    internal enum BindMethod : uint
    {
        LDAP_AUTH_DIGEST = 0x4086,
        LDAP_AUTH_DPA = 0x2086,
        LDAP_AUTH_EXTERNAL = 0xa6,
        LDAP_AUTH_MSN = 0x886,
        LDAP_AUTH_NEGOTIATE = 0x486,
        LDAP_AUTH_NTLM = 0x1086,
        LDAP_AUTH_OTHERKIND = 0x86,
        LDAP_AUTH_SASL = 0x83,
        LDAP_AUTH_SICILY = 0x286,
        LDAP_AUTH_SIMPLE = 0x80,
        LDAP_AUTH_SSPI = 0x486
    }
}

