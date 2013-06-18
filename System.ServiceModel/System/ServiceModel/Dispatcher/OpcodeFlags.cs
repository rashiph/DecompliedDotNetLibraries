namespace System.ServiceModel.Dispatcher
{
    using System;

    internal enum OpcodeFlags
    {
        Branch = 4,
        CompressableSelect = 0x800,
        Deleted = 0x80,
        Fx = 0x1000,
        InConditional = 0x100,
        InitialSelect = 0x400,
        Jump = 0x10,
        Literal = 0x20,
        Multiple = 2,
        NoContextCopy = 0x200,
        None = 0,
        Result = 8,
        Select = 0x40,
        Single = 1
    }
}

