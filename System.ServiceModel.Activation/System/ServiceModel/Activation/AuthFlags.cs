namespace System.ServiceModel.Activation
{
    using System;

    [Flags]
    internal enum AuthFlags
    {
        AuthAnonymous = 1,
        AuthBasic = 2,
        AuthMD5 = 0x10,
        AuthNTLM = 4,
        AuthPassport = 0x40,
        None = 0
    }
}

