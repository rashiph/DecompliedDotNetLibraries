namespace System.Net
{
    using System;

    [Flags]
    internal enum Alg
    {
        Any = 0,
        ClassEncrypt = 0x6000,
        ClassHash = 0x8000,
        ClassKeyXch = 0xa000,
        ClassSignture = 0x2000,
        Name3DES = 3,
        NameAES = 0x11,
        NameAES_128 = 14,
        NameAES_192 = 15,
        NameAES_256 = 0x10,
        NameDES = 1,
        NameDH_Ephem = 2,
        NameMD5 = 3,
        NameRC2 = 2,
        NameRC4 = 1,
        NameSHA = 4,
        TypeBlock = 0x600,
        TypeDH = 0xa00,
        TypeRSA = 0x400,
        TypeStream = 0x800
    }
}

