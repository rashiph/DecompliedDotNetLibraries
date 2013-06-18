namespace System.ServiceProcess
{
    using System;

    [Flags]
    public enum ServiceType
    {
        Adapter = 4,
        FileSystemDriver = 2,
        InteractiveProcess = 0x100,
        KernelDriver = 1,
        RecognizerDriver = 8,
        Win32OwnProcess = 0x10,
        Win32ShareProcess = 0x20
    }
}

