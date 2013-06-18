namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("1113f52d-dc7f-4943-aed6-88d04027e32a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IProcessInitializer
    {
        void Startup([In, MarshalAs(UnmanagedType.IUnknown)] object punkProcessControl);
        void Shutdown();
    }
}

