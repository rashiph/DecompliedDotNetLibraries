namespace System.Security.Cryptography.X509Certificates
{
    using System;

    [Flags]
    public enum X500DistinguishedNameFlags
    {
        DoNotUsePlusSign = 0x20,
        DoNotUseQuotes = 0x40,
        ForceUTF8Encoding = 0x4000,
        None = 0,
        Reversed = 1,
        UseCommas = 0x80,
        UseNewLines = 0x100,
        UseSemicolons = 0x10,
        UseT61Encoding = 0x2000,
        UseUTF8Encoding = 0x1000
    }
}

