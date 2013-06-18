namespace Microsoft.Build.Tasks.Hosting
{
    using Microsoft.Build.Framework;
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("D6D4E228-259A-4076-B5D0-0627338BCC10"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICscHostObject2 : ICscHostObject, ITaskHost
    {
        bool SetWin32Manifest(string win32Manifest);
    }
}

