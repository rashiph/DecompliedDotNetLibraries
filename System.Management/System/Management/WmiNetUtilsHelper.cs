namespace System.Management
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class WmiNetUtilsHelper
    {
        internal static BeginEnumeration BeginEnumeration_f;
        internal static BeginMethodEnumeration BeginMethodEnumeration_f;
        internal static BlessIWbemServices BlessIWbemServices_f;
        internal static BlessIWbemServicesObject BlessIWbemServicesObject_f;
        internal static Clone Clone_f;
        internal static Clone Clone_f12;
        internal static CloneEnumWbemClassObject CloneEnumWbemClassObject_f;
        internal static CompareTo CompareTo_f;
        internal static ConnectServerWmi ConnectServerWmi_f;
        internal static CreateClassEnumWmi CreateClassEnumWmi_f;
        internal static CreateInstanceEnumWmi CreateInstanceEnumWmi_f;
        internal static Delete Delete_f;
        internal static DeleteMethod DeleteMethod_f;
        internal static EndEnumeration EndEnumeration_f;
        internal static EndMethodEnumeration EndMethodEnumeration_f;
        internal static ExecNotificationQueryWmi ExecNotificationQueryWmi_f;
        internal static ExecQueryWmi ExecQueryWmi_f;
        internal static Get Get_f;
        internal static GetCurrentApartmentType GetCurrentApartmentType_f;
        internal static GetDemultiplexedStub GetDemultiplexedStub_f;
        internal static GetMethod GetMethod_f;
        internal static GetMethodOrigin GetMethodOrigin_f;
        internal static GetMethodQualifierSet GetMethodQualifierSet_f;
        internal static GetNames GetNames_f;
        internal static GetObjectText GetObjectText_f;
        internal static GetPropertyHandle GetPropertyHandle_f27;
        internal static GetPropertyOrigin GetPropertyOrigin_f;
        internal static GetPropertyQualifierSet GetPropertyQualifierSet_f;
        internal static GetQualifierSet GetQualifierSet_f;
        internal static InheritsFrom InheritsFrom_f;
        internal static string myDllPath = (RuntimeEnvironment.GetRuntimeDirectory() + @"\wminet_utils.dll");
        internal static Next Next_f;
        internal static NextMethod NextMethod_f;
        internal static Put Put_f;
        internal static PutClassWmi PutClassWmi_f;
        internal static PutInstanceWmi PutInstanceWmi_f;
        internal static PutMethod PutMethod_f;
        internal static QualifierSet_BeginEnumeration QualifierBeginEnumeration_f;
        internal static QualifierSet_Delete QualifierDelete_f;
        internal static QualifierSet_EndEnumeration QualifierEndEnumeration_f;
        internal static QualifierSet_Get QualifierGet_f;
        internal static QualifierSet_GetNames QualifierGetNames_f;
        internal static QualifierSet_Next QualifierNext_f;
        internal static QualifierSet_Put QualifierPut_f;
        internal static ResetSecurity ResetSecurity_f;
        internal static SetSecurity SetSecurity_f;
        internal static SpawnDerivedClass SpawnDerivedClass_f;
        internal static SpawnInstance SpawnInstance_f;
        internal static VerifyClientKey VerifyClientKey_f;
        internal static WritePropertyValue WritePropertyValue_f28;

        static WmiNetUtilsHelper()
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr hModule = IntPtr.Zero;
            hModule = LoadLibrary(myDllPath);
            if (hModule != IntPtr.Zero)
            {
                zero = GetProcAddress(hModule, "ResetSecurity");
                if (zero != IntPtr.Zero)
                {
                    ResetSecurity_f = (ResetSecurity) Marshal.GetDelegateForFunctionPointer(zero, typeof(ResetSecurity));
                }
                zero = GetProcAddress(hModule, "SetSecurity");
                if (zero != IntPtr.Zero)
                {
                    SetSecurity_f = (SetSecurity) Marshal.GetDelegateForFunctionPointer(zero, typeof(SetSecurity));
                }
                zero = GetProcAddress(hModule, "BlessIWbemServices");
                if (zero != IntPtr.Zero)
                {
                    BlessIWbemServices_f = (BlessIWbemServices) Marshal.GetDelegateForFunctionPointer(zero, typeof(BlessIWbemServices));
                }
                zero = GetProcAddress(hModule, "BlessIWbemServicesObject");
                if (zero != IntPtr.Zero)
                {
                    BlessIWbemServicesObject_f = (BlessIWbemServicesObject) Marshal.GetDelegateForFunctionPointer(zero, typeof(BlessIWbemServicesObject));
                }
                zero = GetProcAddress(hModule, "GetPropertyHandle");
                if (zero != IntPtr.Zero)
                {
                    GetPropertyHandle_f27 = (GetPropertyHandle) Marshal.GetDelegateForFunctionPointer(zero, typeof(GetPropertyHandle));
                }
                zero = GetProcAddress(hModule, "WritePropertyValue");
                if (zero != IntPtr.Zero)
                {
                    WritePropertyValue_f28 = (WritePropertyValue) Marshal.GetDelegateForFunctionPointer(zero, typeof(WritePropertyValue));
                }
                zero = GetProcAddress(hModule, "Clone");
                if (zero != IntPtr.Zero)
                {
                    Clone_f12 = (Clone) Marshal.GetDelegateForFunctionPointer(zero, typeof(Clone));
                }
                zero = GetProcAddress(hModule, "VerifyClientKey");
                if (zero != IntPtr.Zero)
                {
                    VerifyClientKey_f = (VerifyClientKey) Marshal.GetDelegateForFunctionPointer(zero, typeof(VerifyClientKey));
                }
                zero = GetProcAddress(hModule, "GetQualifierSet");
                if (zero != IntPtr.Zero)
                {
                    GetQualifierSet_f = (GetQualifierSet) Marshal.GetDelegateForFunctionPointer(zero, typeof(GetQualifierSet));
                }
                zero = GetProcAddress(hModule, "Get");
                if (zero != IntPtr.Zero)
                {
                    Get_f = (Get) Marshal.GetDelegateForFunctionPointer(zero, typeof(Get));
                }
                zero = GetProcAddress(hModule, "Put");
                if (zero != IntPtr.Zero)
                {
                    Put_f = (Put) Marshal.GetDelegateForFunctionPointer(zero, typeof(Put));
                }
                zero = GetProcAddress(hModule, "Delete");
                if (zero != IntPtr.Zero)
                {
                    Delete_f = (Delete) Marshal.GetDelegateForFunctionPointer(zero, typeof(Delete));
                }
                zero = GetProcAddress(hModule, "GetNames");
                if (zero != IntPtr.Zero)
                {
                    GetNames_f = (GetNames) Marshal.GetDelegateForFunctionPointer(zero, typeof(GetNames));
                }
                zero = GetProcAddress(hModule, "BeginEnumeration");
                if (zero != IntPtr.Zero)
                {
                    BeginEnumeration_f = (BeginEnumeration) Marshal.GetDelegateForFunctionPointer(zero, typeof(BeginEnumeration));
                }
                zero = GetProcAddress(hModule, "Next");
                if (zero != IntPtr.Zero)
                {
                    Next_f = (Next) Marshal.GetDelegateForFunctionPointer(zero, typeof(Next));
                }
                zero = GetProcAddress(hModule, "EndEnumeration");
                if (zero != IntPtr.Zero)
                {
                    EndEnumeration_f = (EndEnumeration) Marshal.GetDelegateForFunctionPointer(zero, typeof(EndEnumeration));
                }
                zero = GetProcAddress(hModule, "GetPropertyQualifierSet");
                if (zero != IntPtr.Zero)
                {
                    GetPropertyQualifierSet_f = (GetPropertyQualifierSet) Marshal.GetDelegateForFunctionPointer(zero, typeof(GetPropertyQualifierSet));
                }
                zero = GetProcAddress(hModule, "Clone");
                if (zero != IntPtr.Zero)
                {
                    Clone_f = (Clone) Marshal.GetDelegateForFunctionPointer(zero, typeof(Clone));
                }
                zero = GetProcAddress(hModule, "GetObjectText");
                if (zero != IntPtr.Zero)
                {
                    GetObjectText_f = (GetObjectText) Marshal.GetDelegateForFunctionPointer(zero, typeof(GetObjectText));
                }
                zero = GetProcAddress(hModule, "SpawnDerivedClass");
                if (zero != IntPtr.Zero)
                {
                    SpawnDerivedClass_f = (SpawnDerivedClass) Marshal.GetDelegateForFunctionPointer(zero, typeof(SpawnDerivedClass));
                }
                zero = GetProcAddress(hModule, "SpawnInstance");
                if (zero != IntPtr.Zero)
                {
                    SpawnInstance_f = (SpawnInstance) Marshal.GetDelegateForFunctionPointer(zero, typeof(SpawnInstance));
                }
                zero = GetProcAddress(hModule, "CompareTo");
                if (zero != IntPtr.Zero)
                {
                    CompareTo_f = (CompareTo) Marshal.GetDelegateForFunctionPointer(zero, typeof(CompareTo));
                }
                zero = GetProcAddress(hModule, "GetPropertyOrigin");
                if (zero != IntPtr.Zero)
                {
                    GetPropertyOrigin_f = (GetPropertyOrigin) Marshal.GetDelegateForFunctionPointer(zero, typeof(GetPropertyOrigin));
                }
                zero = GetProcAddress(hModule, "InheritsFrom");
                if (zero != IntPtr.Zero)
                {
                    InheritsFrom_f = (InheritsFrom) Marshal.GetDelegateForFunctionPointer(zero, typeof(InheritsFrom));
                }
                zero = GetProcAddress(hModule, "GetMethod");
                if (zero != IntPtr.Zero)
                {
                    GetMethod_f = (GetMethod) Marshal.GetDelegateForFunctionPointer(zero, typeof(GetMethod));
                }
                zero = GetProcAddress(hModule, "PutMethod");
                if (zero != IntPtr.Zero)
                {
                    PutMethod_f = (PutMethod) Marshal.GetDelegateForFunctionPointer(zero, typeof(PutMethod));
                }
                zero = GetProcAddress(hModule, "DeleteMethod");
                if (zero != IntPtr.Zero)
                {
                    DeleteMethod_f = (DeleteMethod) Marshal.GetDelegateForFunctionPointer(zero, typeof(DeleteMethod));
                }
                zero = GetProcAddress(hModule, "BeginMethodEnumeration");
                if (zero != IntPtr.Zero)
                {
                    BeginMethodEnumeration_f = (BeginMethodEnumeration) Marshal.GetDelegateForFunctionPointer(zero, typeof(BeginMethodEnumeration));
                }
                zero = GetProcAddress(hModule, "NextMethod");
                if (zero != IntPtr.Zero)
                {
                    NextMethod_f = (NextMethod) Marshal.GetDelegateForFunctionPointer(zero, typeof(NextMethod));
                }
                zero = GetProcAddress(hModule, "EndMethodEnumeration");
                if (zero != IntPtr.Zero)
                {
                    EndMethodEnumeration_f = (EndMethodEnumeration) Marshal.GetDelegateForFunctionPointer(zero, typeof(EndMethodEnumeration));
                }
                zero = GetProcAddress(hModule, "GetMethodQualifierSet");
                if (zero != IntPtr.Zero)
                {
                    GetMethodQualifierSet_f = (GetMethodQualifierSet) Marshal.GetDelegateForFunctionPointer(zero, typeof(GetMethodQualifierSet));
                }
                zero = GetProcAddress(hModule, "GetMethodOrigin");
                if (zero != IntPtr.Zero)
                {
                    GetMethodOrigin_f = (GetMethodOrigin) Marshal.GetDelegateForFunctionPointer(zero, typeof(GetMethodOrigin));
                }
                zero = GetProcAddress(hModule, "QualifierSet_Get");
                if (zero != IntPtr.Zero)
                {
                    QualifierGet_f = (QualifierSet_Get) Marshal.GetDelegateForFunctionPointer(zero, typeof(QualifierSet_Get));
                }
                zero = GetProcAddress(hModule, "QualifierSet_Put");
                if (zero != IntPtr.Zero)
                {
                    QualifierPut_f = (QualifierSet_Put) Marshal.GetDelegateForFunctionPointer(zero, typeof(QualifierSet_Put));
                }
                zero = GetProcAddress(hModule, "QualifierSet_Delete");
                if (zero != IntPtr.Zero)
                {
                    QualifierDelete_f = (QualifierSet_Delete) Marshal.GetDelegateForFunctionPointer(zero, typeof(QualifierSet_Delete));
                }
                zero = GetProcAddress(hModule, "QualifierSet_GetNames");
                if (zero != IntPtr.Zero)
                {
                    QualifierGetNames_f = (QualifierSet_GetNames) Marshal.GetDelegateForFunctionPointer(zero, typeof(QualifierSet_GetNames));
                }
                zero = GetProcAddress(hModule, "QualifierSet_BeginEnumeration");
                if (zero != IntPtr.Zero)
                {
                    QualifierBeginEnumeration_f = (QualifierSet_BeginEnumeration) Marshal.GetDelegateForFunctionPointer(zero, typeof(QualifierSet_BeginEnumeration));
                }
                zero = GetProcAddress(hModule, "QualifierSet_Next");
                if (zero != IntPtr.Zero)
                {
                    QualifierNext_f = (QualifierSet_Next) Marshal.GetDelegateForFunctionPointer(zero, typeof(QualifierSet_Next));
                }
                zero = GetProcAddress(hModule, "QualifierSet_EndEnumeration");
                if (zero != IntPtr.Zero)
                {
                    QualifierEndEnumeration_f = (QualifierSet_EndEnumeration) Marshal.GetDelegateForFunctionPointer(zero, typeof(QualifierSet_EndEnumeration));
                }
                zero = GetProcAddress(hModule, "GetCurrentApartmentType");
                if (zero != IntPtr.Zero)
                {
                    GetCurrentApartmentType_f = (GetCurrentApartmentType) Marshal.GetDelegateForFunctionPointer(zero, typeof(GetCurrentApartmentType));
                }
                zero = GetProcAddress(hModule, "GetDemultiplexedStub");
                if (zero != IntPtr.Zero)
                {
                    GetDemultiplexedStub_f = (GetDemultiplexedStub) Marshal.GetDelegateForFunctionPointer(zero, typeof(GetDemultiplexedStub));
                }
                zero = GetProcAddress(hModule, "CreateInstanceEnumWmi");
                if (zero != IntPtr.Zero)
                {
                    CreateInstanceEnumWmi_f = (CreateInstanceEnumWmi) Marshal.GetDelegateForFunctionPointer(zero, typeof(CreateInstanceEnumWmi));
                }
                zero = GetProcAddress(hModule, "CreateClassEnumWmi");
                if (zero != IntPtr.Zero)
                {
                    CreateClassEnumWmi_f = (CreateClassEnumWmi) Marshal.GetDelegateForFunctionPointer(zero, typeof(CreateClassEnumWmi));
                }
                zero = GetProcAddress(hModule, "ExecQueryWmi");
                if (zero != IntPtr.Zero)
                {
                    ExecQueryWmi_f = (ExecQueryWmi) Marshal.GetDelegateForFunctionPointer(zero, typeof(ExecQueryWmi));
                }
                zero = GetProcAddress(hModule, "ExecNotificationQueryWmi");
                if (zero != IntPtr.Zero)
                {
                    ExecNotificationQueryWmi_f = (ExecNotificationQueryWmi) Marshal.GetDelegateForFunctionPointer(zero, typeof(ExecNotificationQueryWmi));
                }
                zero = GetProcAddress(hModule, "PutInstanceWmi");
                if (zero != IntPtr.Zero)
                {
                    PutInstanceWmi_f = (PutInstanceWmi) Marshal.GetDelegateForFunctionPointer(zero, typeof(PutInstanceWmi));
                }
                zero = GetProcAddress(hModule, "PutClassWmi");
                if (zero != IntPtr.Zero)
                {
                    PutClassWmi_f = (PutClassWmi) Marshal.GetDelegateForFunctionPointer(zero, typeof(PutClassWmi));
                }
                zero = GetProcAddress(hModule, "CloneEnumWbemClassObject");
                if (zero != IntPtr.Zero)
                {
                    CloneEnumWbemClassObject_f = (CloneEnumWbemClassObject) Marshal.GetDelegateForFunctionPointer(zero, typeof(CloneEnumWbemClassObject));
                }
                zero = GetProcAddress(hModule, "ConnectServerWmi");
                if (zero != IntPtr.Zero)
                {
                    ConnectServerWmi_f = (ConnectServerWmi) Marshal.GetDelegateForFunctionPointer(zero, typeof(ConnectServerWmi));
                }
            }
        }

        [SuppressUnmanagedCodeSecurity, DllImport("kernel32.dll")]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procname);
        [SuppressUnmanagedCodeSecurity, DllImport("kernel32.dll")]
        internal static extern IntPtr LoadLibrary(string fileName);

        internal enum APTTYPE
        {
            APTTYPE_CURRENT = -1,
            APTTYPE_MAINSTA = 3,
            APTTYPE_MTA = 1,
            APTTYPE_NA = 2,
            APTTYPE_STA = 0
        }

        internal delegate int BeginEnumeration(int vFunc, IntPtr pWbemClassObject, [In] int lEnumFlags);

        internal delegate int BeginMethodEnumeration(int vFunc, IntPtr pWbemClassObject, [In] int lEnumFlags);

        internal delegate int BlessIWbemServices([MarshalAs(UnmanagedType.Interface)] IWbemServices pIUnknown, [In, MarshalAs(UnmanagedType.BStr)] string strUser, IntPtr password, [In, MarshalAs(UnmanagedType.BStr)] string strAuthority, int impersonationLevel, int authenticationLevel);

        internal delegate int BlessIWbemServicesObject([MarshalAs(UnmanagedType.IUnknown)] object pIUnknown, [In, MarshalAs(UnmanagedType.BStr)] string strUser, IntPtr password, [In, MarshalAs(UnmanagedType.BStr)] string strAuthority, int impersonationLevel, int authenticationLevel);

        internal delegate int Clone(int vFunc, IntPtr pWbemClassObject, out IntPtr ppCopy);

        internal delegate int CloneEnumWbemClassObject([MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum, [In] int impLevel, [In] int authnLevel, [In, MarshalAs(UnmanagedType.Interface)] IEnumWbemClassObject pCurrentEnumWbemClassObject, [In, MarshalAs(UnmanagedType.BStr)] string strUser, [In] IntPtr strPassword, [In, MarshalAs(UnmanagedType.BStr)] string strAuthority);

        internal delegate int CompareTo(int vFunc, IntPtr pWbemClassObject, [In] int lFlags, [In] IntPtr pCompareTo);

        internal delegate int ConnectServerWmi([In, MarshalAs(UnmanagedType.BStr)] string strNetworkResource, [In, MarshalAs(UnmanagedType.BStr)] string strUser, [In] IntPtr strPassword, [In, MarshalAs(UnmanagedType.BStr)] string strLocale, [In] int lSecurityFlags, [In, MarshalAs(UnmanagedType.BStr)] string strAuthority, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IWbemServices ppNamespace, int impersonationLevel, int authenticationLevel);

        internal delegate int CreateClassEnumWmi([In, MarshalAs(UnmanagedType.BStr)] string strSuperclass, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum, [In] int impLevel, [In] int authnLevel, [In, MarshalAs(UnmanagedType.Interface)] IWbemServices pCurrentNamespace, [In, MarshalAs(UnmanagedType.BStr)] string strUser, [In] IntPtr strPassword, [In, MarshalAs(UnmanagedType.BStr)] string strAuthority);

        internal delegate int CreateInstanceEnumWmi([In, MarshalAs(UnmanagedType.BStr)] string strFilter, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum, [In] int impLevel, [In] int authnLevel, [In, MarshalAs(UnmanagedType.Interface)] IWbemServices pCurrentNamespace, [In, MarshalAs(UnmanagedType.BStr)] string strUser, [In] IntPtr strPassword, [In, MarshalAs(UnmanagedType.BStr)] string strAuthority);

        internal delegate int Delete(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszName);

        internal delegate int DeleteMethod(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszName);

        internal delegate int EndEnumeration(int vFunc, IntPtr pWbemClassObject);

        internal delegate int EndMethodEnumeration(int vFunc, IntPtr pWbemClassObject);

        internal delegate int ExecNotificationQueryWmi([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum, [In] int impLevel, [In] int authnLevel, [In, MarshalAs(UnmanagedType.Interface)] IWbemServices pCurrentNamespace, [In, MarshalAs(UnmanagedType.BStr)] string strUser, [In] IntPtr strPassword, [In, MarshalAs(UnmanagedType.BStr)] string strAuthority);

        internal delegate int ExecQueryWmi([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum, [In] int impLevel, [In] int authnLevel, [In, MarshalAs(UnmanagedType.Interface)] IWbemServices pCurrentNamespace, [In, MarshalAs(UnmanagedType.BStr)] string strUser, [In] IntPtr strPassword, [In, MarshalAs(UnmanagedType.BStr)] string strAuthority);

        internal delegate int Get(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In, Out] ref object pVal, [In, Out] ref int pType, [In, Out] ref int plFlavor);

        internal delegate int GetCurrentApartmentType(int vFunc, IntPtr pComThreadingInfo, out WmiNetUtilsHelper.APTTYPE aptType);

        internal delegate int GetDemultiplexedStub([In, MarshalAs(UnmanagedType.IUnknown)] object pIUnknown, [In] bool isLocal, [MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknown);

        internal delegate int GetMethod(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, out IntPtr ppInSignature, out IntPtr ppOutSignature);

        internal delegate int GetMethodOrigin(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszMethodName, [MarshalAs(UnmanagedType.BStr)] out string pstrClassName);

        internal delegate int GetMethodQualifierSet(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszMethod, out IntPtr ppQualSet);

        internal delegate int GetNames(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszQualifierName, [In] int lFlags, [In] ref object pQualifierVal, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_BSTR)] out string[] pNames);

        internal delegate int GetObjectText(int vFunc, IntPtr pWbemClassObject, [In] int lFlags, [MarshalAs(UnmanagedType.BStr)] out string pstrObjectText);

        internal delegate int GetPropertyHandle(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszPropertyName, out int pType, out int plHandle);

        internal delegate int GetPropertyOrigin(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [MarshalAs(UnmanagedType.BStr)] out string pstrClassName);

        internal delegate int GetPropertyQualifierSet(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszProperty, out IntPtr ppQualSet);

        internal delegate int GetQualifierSet(int vFunc, IntPtr pWbemClassObject, out IntPtr ppQualSet);

        internal delegate int InheritsFrom(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string strAncestor);

        internal delegate int Next(int vFunc, IntPtr pWbemClassObject, [In] int lFlags, [In, Out, MarshalAs(UnmanagedType.BStr)] ref string strName, [In, Out] ref object pVal, [In, Out] ref int pType, [In, Out] ref int plFlavor);

        internal delegate int NextMethod(int vFunc, IntPtr pWbemClassObject, [In] int lFlags, [MarshalAs(UnmanagedType.BStr)] out string pstrName, out IntPtr ppInSignature, out IntPtr ppOutSignature);

        internal delegate int Put(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In] ref object pVal, [In] int Type);

        internal delegate int PutClassWmi([In] IntPtr pObject, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In] IntPtr ppCallResult, [In] int impLevel, [In] int authnLevel, [In, MarshalAs(UnmanagedType.Interface)] IWbemServices pCurrentNamespace, [In, MarshalAs(UnmanagedType.BStr)] string strUser, [In] IntPtr strPassword, [In, MarshalAs(UnmanagedType.BStr)] string strAuthority);

        internal delegate int PutInstanceWmi([In] IntPtr pInst, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In] IntPtr ppCallResult, [In] int impLevel, [In] int authnLevel, [In, MarshalAs(UnmanagedType.Interface)] IWbemServices pCurrentNamespace, [In, MarshalAs(UnmanagedType.BStr)] string strUser, [In] IntPtr strPassword, [In, MarshalAs(UnmanagedType.BStr)] string strAuthority);

        internal delegate int PutMethod(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In] IntPtr pInSignature, [In] IntPtr pOutSignature);

        internal delegate int QualifierSet_BeginEnumeration(int vFunc, IntPtr pWbemClassObject, [In] int lFlags);

        internal delegate int QualifierSet_Delete(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszName);

        internal delegate int QualifierSet_EndEnumeration(int vFunc, IntPtr pWbemClassObject);

        internal delegate int QualifierSet_Get(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In, Out] ref object pVal, [In, Out] ref int plFlavor);

        internal delegate int QualifierSet_GetNames(int vFunc, IntPtr pWbemClassObject, [In] int lFlags, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_BSTR)] out string[] pNames);

        internal delegate int QualifierSet_Next(int vFunc, IntPtr pWbemClassObject, [In] int lFlags, [MarshalAs(UnmanagedType.BStr)] out string pstrName, out object pVal, out int plFlavor);

        internal delegate int QualifierSet_Put(int vFunc, IntPtr pWbemClassObject, [In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] ref object pVal, [In] int lFlavor);

        internal delegate int ResetSecurity(IntPtr hToken);

        internal delegate int SetSecurity([In, Out] ref bool pNeedtoReset, [In, Out] ref IntPtr pHandle);

        internal delegate int SpawnDerivedClass(int vFunc, IntPtr pWbemClassObject, [In] int lFlags, out IntPtr ppNewClass);

        internal delegate int SpawnInstance(int vFunc, IntPtr pWbemClassObject, [In] int lFlags, out IntPtr ppNewInstance);

        internal delegate void VerifyClientKey();

        internal delegate int WritePropertyValue(int vFunc, IntPtr pWbemClassObject, [In] int lHandle, [In] int lNumBytes, [In, MarshalAs(UnmanagedType.LPWStr)] string str);
    }
}

