namespace System.Security.Cryptography
{
    using System;

    [Flags]
    public enum CngUIProtectionLevels
    {
        None,
        ProtectKey,
        ForceHighProtection
    }
}

