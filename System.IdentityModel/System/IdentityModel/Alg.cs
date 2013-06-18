namespace System.IdentityModel
{
    using System;

    internal enum Alg
    {
        Any = 0,
        ClassEncrypt = 0x6000,
        ClassHash = 0x8000,
        ClassKeyXch = 0xa000,
        ClassSignture = 0x2000,
        Fortezza = 4,
        NameDES = 1,
        NameDH_Ephem = 2,
        NameRC2 = 2,
        NameRC4 = 1,
        NameSHA = 4,
        NameSkipJack = 10,
        TypeBlock = 0x600,
        TypeDH = 0xa00,
        TypeRSA = 0x400,
        TypeStream = 0x800
    }
}

