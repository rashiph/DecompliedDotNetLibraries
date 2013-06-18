namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x200), InterfaceType((short) 1), Guid("3AE0080A-7E3A-4366-BF89-0FEEDC931659")]
    internal interface IWbemEventSink
    {
        [PreserveSig]
        int Indicate_([In] int lObjectCount, [In, MarshalAs(UnmanagedType.Interface)] ref IWbemClassObject_DoNotMarshal apObjArray);
        [PreserveSig]
        int SetStatus_([In] int lFlags, [In, MarshalAs(UnmanagedType.Error)] int hResult, [In, MarshalAs(UnmanagedType.BStr)] string strParam, [In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pObjParam);
        [PreserveSig]
        int IndicateWithSD_([In] int lNumObjects, [In, MarshalAs(UnmanagedType.IUnknown)] ref object apObjects, [In] int lSDLength, [In] ref byte pSD);
        [PreserveSig]
        int SetSinkSecurity_([In] int lSDLength, [In] ref byte pSD);
        [PreserveSig]
        int IsActive_();
        [PreserveSig]
        int GetRestrictedSink_([In] int lNumQueries, [In, MarshalAs(UnmanagedType.LPWStr)] ref string awszQueries, [In, MarshalAs(UnmanagedType.IUnknown)] object pCallback, [MarshalAs(UnmanagedType.Interface)] out IWbemEventSink ppSink);
        [PreserveSig]
        int SetBatchingParameters_([In] int lFlags, [In] uint dwMaxBufferSize, [In] uint dwMaxSendLatency);
    }
}

