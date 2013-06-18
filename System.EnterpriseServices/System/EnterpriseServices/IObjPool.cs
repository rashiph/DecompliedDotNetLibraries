namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7D8805A0-2EA7-11D1-B1CC-00AA00BA3258")]
    internal interface IObjPool
    {
        void Init([MarshalAs(UnmanagedType.Interface)] object pClassInfo);
        [return: MarshalAs(UnmanagedType.Interface)]
        object Get();
        void SetOption(int eOption, int dwOption);
        void PutNew([In, MarshalAs(UnmanagedType.Interface)] object pObj);
        void PutEndTx([In, MarshalAs(UnmanagedType.Interface)] object pObj);
        void PutDeactivated([In, MarshalAs(UnmanagedType.Interface)] object pObj);
        void Shutdown();
    }
}

