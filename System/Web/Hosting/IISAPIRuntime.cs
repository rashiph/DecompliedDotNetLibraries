namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("08a2c56f-7c16-41c1-a8be-432917a1a2d1")]
    public interface IISAPIRuntime
    {
        void StartProcessing();
        void StopProcessing();
        [return: MarshalAs(UnmanagedType.I4)]
        int ProcessRequest([In] IntPtr ecb, [In, MarshalAs(UnmanagedType.I4)] int useProcessModel);
        void DoGCCollect();
    }
}

