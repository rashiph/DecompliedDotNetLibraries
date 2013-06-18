namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("DC12A681-737F-11CF-884D-00AA004B2E24"), TypeLibType((short) 0x200)]
    internal interface IWbemClassObject_DoNotMarshal
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
    }
}

