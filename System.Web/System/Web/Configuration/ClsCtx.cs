namespace System.Web.Configuration
{
    using System;

    internal enum ClsCtx
    {
        All = 0x17,
        DisableAAA = 0x8000,
        EnableAAA = 0x10000,
        EnableCodeDownload = 0x2000,
        EServerHandler = 0x100,
        FromDefaultContext = 0x20000,
        Inproc = 3,
        InprocHandler = 2,
        InprocHandler16 = 0x20,
        InprocHandlerX86 = 0x80,
        InprocServer = 1,
        InprocServer16 = 8,
        InprocServerX86 = 0x40,
        LocalServer = 4,
        NoCodeDownload = 0x400,
        NoCustomMarshal = 0x1000,
        NoFailureLog = 0x4000,
        NoWX86Translation = 0x800,
        RemoteServer = 0x10,
        Reserved = 0x200,
        Server = 0x15
    }
}

