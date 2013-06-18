namespace Microsoft.Build.Tasks.Hosting
{
    using Microsoft.Build.Framework;
    using System;
    using System.Runtime.InteropServices;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("f59afc84-d102-48b1-a090-1b90c79d3e09"), ComVisible(true)]
    public interface IVbcHostObject2 : IVbcHostObject, ITaskHost
    {
        bool SetOptionInfer(bool optionInfer);
        bool SetModuleAssemblyName(string moduleAssemblyName);
        bool SetWin32Manifest(string win32Manifest);
    }
}

