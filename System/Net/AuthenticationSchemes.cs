namespace System.Net
{
    using System;

    [Flags]
    public enum AuthenticationSchemes
    {
        Anonymous = 0x8000,
        Basic = 8,
        Digest = 1,
        IntegratedWindowsAuthentication = 6,
        Negotiate = 2,
        None = 0,
        Ntlm = 4
    }
}

