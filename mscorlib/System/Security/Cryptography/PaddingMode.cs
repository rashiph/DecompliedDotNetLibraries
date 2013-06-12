namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum PaddingMode
    {
        ANSIX923 = 4,
        ISO10126 = 5,
        None = 1,
        PKCS7 = 2,
        Zeros = 3
    }
}

