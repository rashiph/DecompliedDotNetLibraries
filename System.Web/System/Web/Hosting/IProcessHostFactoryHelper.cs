namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("02fd465d-5c5d-4b7e-95b6-82faa031b74a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IProcessHostFactoryHelper
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetProcessHost(IProcessHostSupportFunctions functions);
    }
}

