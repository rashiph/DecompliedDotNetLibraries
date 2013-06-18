namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000001ce-0000-0000-C000-000000000046")]
    internal interface IComThreadingInfo
    {
        void GetCurrentApartmentType(out uint aptType);
        void GetCurrentThreadType(out uint threadType);
        void GetCurrentLogicalThreadId(out Guid guidLogicalThreadID);
        void SetCurrentLogicalThreadId([In, MarshalAs(UnmanagedType.LPStruct)] Guid guidLogicalThreadID);
    }
}

