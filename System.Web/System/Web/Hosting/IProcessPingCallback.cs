namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("f11dc4c9-ddd1-4566-ad53-cf6f3a28fefe")]
    public interface IProcessPingCallback
    {
        void Respond();
    }
}

