namespace System.Reflection
{
    using System;

    [Serializable, Flags]
    internal enum MdSigCallingConvention : byte
    {
        C = 1,
        CallConvMask = 15,
        Default = 0,
        ExplicitThis = 0x40,
        FastCall = 4,
        Field = 6,
        Generic = 0x10,
        GenericInst = 10,
        HasThis = 0x20,
        LocalSig = 7,
        Property = 8,
        StdCall = 2,
        ThisCall = 3,
        Unmgd = 9,
        Vararg = 5
    }
}

