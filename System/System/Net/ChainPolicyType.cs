namespace System.Net
{
    using System;

    internal enum ChainPolicyType
    {
        Authenticode = 2,
        Authenticode_TS = 3,
        Base = 1,
        BasicConstraints = 5,
        NtAuth = 6,
        SSL = 4
    }
}

