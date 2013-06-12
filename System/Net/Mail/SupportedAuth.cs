namespace System.Net.Mail
{
    using System;

    internal enum SupportedAuth
    {
        GSSAPI = 4,
        Login = 1,
        None = 0,
        NTLM = 2,
        WDigest = 8
    }
}

