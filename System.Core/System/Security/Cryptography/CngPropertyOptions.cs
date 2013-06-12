namespace System.Security.Cryptography
{
    using System;

    [Flags]
    public enum CngPropertyOptions
    {
        CustomProperty = 0x40000000,
        None = 0,
        Persist = -2147483648
    }
}

