namespace System.Runtime.InteropServices
{
    using System;

    [Flags]
    public enum RegistrationClassContext
    {
        DisableActivateAsActivator = 0x8000,
        EnableActivateAsActivator = 0x10000,
        EnableCodeDownload = 0x2000,
        FromDefaultContext = 0x20000,
        InProcessHandler = 2,
        InProcessHandler16 = 0x20,
        InProcessServer = 1,
        InProcessServer16 = 8,
        LocalServer = 4,
        NoCodeDownload = 0x400,
        NoCustomMarshal = 0x1000,
        NoFailureLog = 0x4000,
        RemoteServer = 0x10,
        Reserved1 = 0x40,
        Reserved2 = 0x80,
        Reserved3 = 0x100,
        Reserved4 = 0x200,
        Reserved5 = 0x800
    }
}

