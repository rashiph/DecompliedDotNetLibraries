namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x200), Guid("9556DC99-828C-11CF-A37E-00AA003240C7"), InterfaceType((short) 1)]
    internal interface IWbemServices
    {
        [PreserveSig]
        int OpenNamespace_([In, MarshalAs(UnmanagedType.BStr)] string strNamespace, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, Out, MarshalAs(UnmanagedType.Interface)] ref IWbemServices ppWorkingNamespace, [In] IntPtr ppCallResult);
        [PreserveSig]
        int CancelAsyncCall_([In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pSink);
        [PreserveSig]
        int QueryObjectSink_([In] int lFlags, [MarshalAs(UnmanagedType.Interface)] out IWbemObjectSink ppResponseHandler);
        [PreserveSig]
        int GetObject_([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(MarshalWbemObject), MarshalCookie="")] out IWbemClassObjectFreeThreaded ppObject, [In] IntPtr ppCallResult);
        [PreserveSig]
        int GetObjectAsync_([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler);
        [PreserveSig]
        int PutClass_([In] IntPtr pObject, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In] IntPtr ppCallResult);
        [PreserveSig]
        int PutClassAsync_([In] IntPtr pObject, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler);
        [PreserveSig]
        int DeleteClass_([In, MarshalAs(UnmanagedType.BStr)] string strClass, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In] IntPtr ppCallResult);
        [PreserveSig]
        int DeleteClassAsync_([In, MarshalAs(UnmanagedType.BStr)] string strClass, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler);
        [PreserveSig]
        int CreateClassEnum_([In, MarshalAs(UnmanagedType.BStr)] string strSuperclass, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum);
        [PreserveSig]
        int CreateClassEnumAsync_([In, MarshalAs(UnmanagedType.BStr)] string strSuperclass, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler);
        [PreserveSig]
        int PutInstance_([In] IntPtr pInst, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In] IntPtr ppCallResult);
        [PreserveSig]
        int PutInstanceAsync_([In] IntPtr pInst, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler);
        [PreserveSig]
        int DeleteInstance_([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In] IntPtr ppCallResult);
        [PreserveSig]
        int DeleteInstanceAsync_([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler);
        [PreserveSig]
        int CreateInstanceEnum_([In, MarshalAs(UnmanagedType.BStr)] string strFilter, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum);
        [PreserveSig]
        int CreateInstanceEnumAsync_([In, MarshalAs(UnmanagedType.BStr)] string strFilter, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler);
        [PreserveSig]
        int ExecQuery_([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum);
        [PreserveSig]
        int ExecQueryAsync_([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler);
        [PreserveSig]
        int ExecNotificationQuery_([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum);
        [PreserveSig]
        int ExecNotificationQueryAsync_([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler);
        [PreserveSig]
        int ExecMethod_([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In, MarshalAs(UnmanagedType.BStr)] string strMethodName, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In] IntPtr pInParams, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(MarshalWbemObject), MarshalCookie="")] out IWbemClassObjectFreeThreaded ppOutParams, [In] IntPtr ppCallResult);
        [PreserveSig]
        int ExecMethodAsync_([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In, MarshalAs(UnmanagedType.BStr)] string strMethodName, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In] IntPtr pInParams, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler);
    }
}

