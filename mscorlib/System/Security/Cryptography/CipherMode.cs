namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum CipherMode
    {
        CBC = 1,
        CFB = 4,
        CTS = 5,
        ECB = 2,
        OFB = 3
    }
}

