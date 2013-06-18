namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("49353C9A-516B-11D1-AEA6-00C04FB68820"), TypeLibType((short) 0x200)]
    internal interface IWbemObjectAccess
    {
        [PreserveSig]
        int GetQualifierSet_([MarshalAs(UnmanagedType.Interface)] out IWbemQualifierSet_DoNotMarshal ppQualSet);
        [PreserveSig]
        int Get_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In, Out] ref object pVal, [In, Out] ref int pType, [In, Out] ref int plFlavor);
        [PreserveSig]
        int Put_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In] ref object pVal, [In] int Type);
        [PreserveSig]
        int Delete_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName);
        [PreserveSig]
        int GetNames_([In, MarshalAs(UnmanagedType.LPWStr)] string wszQualifierName, [In] int lFlags, [In] ref object pQualifierVal, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_BSTR)] out string[] pNames);
        [PreserveSig]
        int BeginEnumeration_([In] int lEnumFlags);
        [PreserveSig]
        int Next_([In] int lFlags, [In, Out, MarshalAs(UnmanagedType.BStr)] ref string strName, [In, Out] ref object pVal, [In, Out] ref int pType, [In, Out] ref int plFlavor);
        [PreserveSig]
        int EndEnumeration_();
        [PreserveSig]
        int GetPropertyQualifierSet_([In, MarshalAs(UnmanagedType.LPWStr)] string wszProperty, [MarshalAs(UnmanagedType.Interface)] out IWbemQualifierSet_DoNotMarshal ppQualSet);
        [PreserveSig]
        int Clone_([MarshalAs(UnmanagedType.Interface)] out IWbemClassObject_DoNotMarshal ppCopy);
        [PreserveSig]
        int GetObjectText_([In] int lFlags, [MarshalAs(UnmanagedType.BStr)] out string pstrObjectText);
        [PreserveSig]
        int SpawnDerivedClass_([In] int lFlags, [MarshalAs(UnmanagedType.Interface)] out IWbemClassObject_DoNotMarshal ppNewClass);
        [PreserveSig]
        int SpawnInstance_([In] int lFlags, [MarshalAs(UnmanagedType.Interface)] out IWbemClassObject_DoNotMarshal ppNewInstance);
        [PreserveSig]
        int CompareTo_([In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pCompareTo);
        [PreserveSig]
        int GetPropertyOrigin_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [MarshalAs(UnmanagedType.BStr)] out string pstrClassName);
        [PreserveSig]
        int InheritsFrom_([In, MarshalAs(UnmanagedType.LPWStr)] string strAncestor);
        [PreserveSig]
        int GetMethod_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [MarshalAs(UnmanagedType.Interface)] out IWbemClassObject_DoNotMarshal ppInSignature, [MarshalAs(UnmanagedType.Interface)] out IWbemClassObject_DoNotMarshal ppOutSignature);
        [PreserveSig]
        int PutMethod_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pInSignature, [In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pOutSignature);
        [PreserveSig]
        int DeleteMethod_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName);
        [PreserveSig]
        int BeginMethodEnumeration_([In] int lEnumFlags);
        [PreserveSig]
        int NextMethod_([In] int lFlags, [In, Out, MarshalAs(UnmanagedType.BStr)] ref string pstrName, [In, Out, MarshalAs(UnmanagedType.Interface)] ref IWbemClassObject_DoNotMarshal ppInSignature, [In, Out, MarshalAs(UnmanagedType.Interface)] ref IWbemClassObject_DoNotMarshal ppOutSignature);
        [PreserveSig]
        int EndMethodEnumeration_();
        [PreserveSig]
        int GetMethodQualifierSet_([In, MarshalAs(UnmanagedType.LPWStr)] string wszMethod, [MarshalAs(UnmanagedType.Interface)] out IWbemQualifierSet_DoNotMarshal ppQualSet);
        [PreserveSig]
        int GetMethodOrigin_([In, MarshalAs(UnmanagedType.LPWStr)] string wszMethodName, [MarshalAs(UnmanagedType.BStr)] out string pstrClassName);
        [PreserveSig]
        int GetPropertyHandle_([In, MarshalAs(UnmanagedType.LPWStr)] string wszPropertyName, out int pType, out int plHandle);
        [PreserveSig]
        int WritePropertyValue_([In] int lHandle, [In] int lNumBytes, [In] ref byte aData);
        [PreserveSig]
        int ReadPropertyValue_([In] int lHandle, [In] int lBufferSize, out int plNumBytes, out byte aData);
        [PreserveSig]
        int ReadDWORD_([In] int lHandle, out uint pdw);
        [PreserveSig]
        int WriteDWORD_([In] int lHandle, [In] uint dw);
        [PreserveSig]
        int ReadQWORD_([In] int lHandle, out ulong pqw);
        [PreserveSig]
        int WriteQWORD_([In] int lHandle, [In] ulong pw);
        [PreserveSig]
        int GetPropertyInfoByHandle_([In] int lHandle, [MarshalAs(UnmanagedType.BStr)] out string pstrName, out int pType);
        [PreserveSig]
        int Lock_([In] int lFlags);
        [PreserveSig]
        int Unlock_([In] int lFlags);
    }
}

