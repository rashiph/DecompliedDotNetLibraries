namespace System.ServiceModel.Administration
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal class WbemNative
    {
        public enum CIMTYPE
        {
            CIM_BOOLEAN = 11,
            CIM_CHAR16 = 0x67,
            CIM_DATETIME = 0x65,
            CIM_EMPTY = 0,
            CIM_FLAG_ARRAY = 0x2000,
            CIM_ILLEGAL = 0xfff,
            CIM_OBJECT = 13,
            CIM_REAL32 = 4,
            CIM_REAL64 = 5,
            CIM_REFERENCE = 0x66,
            CIM_SINT16 = 2,
            CIM_SINT32 = 3,
            CIM_SINT64 = 20,
            CIM_SINT8 = 0x10,
            CIM_STRING = 8,
            CIM_UINT16 = 0x12,
            CIM_UINT32 = 0x13,
            CIM_UINT64 = 0x15,
            CIM_UINT8 = 0x11
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("027947E1-D731-11CE-A357-000000000001")]
        internal interface IEnumWbemClassObject
        {
            [PreserveSig]
            int Reset();
            [PreserveSig]
            int Next([In] int lTimeout, [In] uint uCount, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] WbemNative.IWbemClassObject[] apObjects, out uint puReturned);
            [PreserveSig]
            int NextAsync([In] uint uCount, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemObjectSink pSink);
            [PreserveSig]
            int Clone([MarshalAs(UnmanagedType.Interface)] out WbemNative.IEnumWbemClassObject ppEnum);
            [PreserveSig]
            int Skip([In] int lTimeout, [In] uint nCount);
        }

        [ComImport, Guid("DC12A681-737F-11CF-884D-00AA004B2E24"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IWbemClassObject
        {
            [PreserveSig]
            int GetQualifierSet([MarshalAs(UnmanagedType.Interface)] out WbemNative.IWbemQualifierSet ppQualSet);
            [PreserveSig]
            int Get([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In, Out] ref object pVal, [In, Out] ref int pType, [In, Out] ref int plFlavor);
            [PreserveSig]
            int Put([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In] ref object pVal, [In] int Type);
            [PreserveSig]
            int Delete([In, MarshalAs(UnmanagedType.LPWStr)] string wszName);
            [PreserveSig]
            int GetNames([In, MarshalAs(UnmanagedType.LPWStr)] string wszQualifierName, [In] int lFlags, [In] ref object pQualifierVal, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_BSTR)] out string[] pNames);
            [PreserveSig]
            int BeginEnumeration([In] int lEnumFlags);
            [PreserveSig]
            int Next([In] int lFlags, [In, Out, MarshalAs(UnmanagedType.BStr)] ref string strName, [In, Out] ref object pVal, [In, Out] ref int pType, [In, Out] ref int plFlavor);
            [PreserveSig]
            int EndEnumeration();
            [PreserveSig]
            int GetPropertyQualifierSet([In, MarshalAs(UnmanagedType.LPWStr)] string wszProperty, [MarshalAs(UnmanagedType.Interface)] out WbemNative.IWbemQualifierSet ppQualSet);
            [PreserveSig]
            int Clone([MarshalAs(UnmanagedType.Interface)] out WbemNative.IWbemClassObject ppCopy);
            [PreserveSig]
            int GetObjectText([In] int lFlags, [MarshalAs(UnmanagedType.BStr)] out string pstrObjectText);
            [PreserveSig]
            int SpawnDerivedClass([In] int lFlags, [MarshalAs(UnmanagedType.Interface)] out WbemNative.IWbemClassObject ppNewClass);
            [PreserveSig]
            int SpawnInstance([In] int lFlags, [MarshalAs(UnmanagedType.Interface)] out WbemNative.IWbemClassObject ppNewInstance);
            [PreserveSig]
            int CompareTo([In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemClassObject pCompareTo);
            [PreserveSig]
            int GetPropertyOrigin([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [MarshalAs(UnmanagedType.BStr)] out string pstrClassName);
            [PreserveSig]
            int InheritsFrom([In, MarshalAs(UnmanagedType.LPWStr)] string strAncestor);
            [PreserveSig]
            int GetMethod([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In] IntPtr ppInSignature, [MarshalAs(UnmanagedType.Interface)] out WbemNative.IWbemClassObject ppOutSignature);
            [PreserveSig]
            int PutMethod([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemClassObject pInSignature, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemClassObject pOutSignature);
            [PreserveSig]
            int DeleteMethod([In, MarshalAs(UnmanagedType.LPWStr)] string wszName);
            [PreserveSig]
            int BeginMethodEnumeration([In] int lEnumFlags);
            [PreserveSig]
            int NextMethod([In] int lFlags, [In, Out, MarshalAs(UnmanagedType.BStr)] ref string pstrName, [In, Out, MarshalAs(UnmanagedType.Interface)] ref WbemNative.IWbemClassObject ppInSignature, [In, Out, MarshalAs(UnmanagedType.Interface)] ref WbemNative.IWbemClassObject ppOutSignature);
            [PreserveSig]
            int EndMethodEnumeration();
            [PreserveSig]
            int GetMethodQualifierSet([In, MarshalAs(UnmanagedType.LPWStr)] string wszMethod, [MarshalAs(UnmanagedType.Interface)] out WbemNative.IWbemQualifierSet ppQualSet);
            [PreserveSig]
            int GetMethodOrigin([In, MarshalAs(UnmanagedType.LPWStr)] string wszMethodName, [MarshalAs(UnmanagedType.BStr)] out string pstrClassName);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("44ACA674-E8FC-11D0-A07C-00C04FB68820")]
        internal interface IWbemContext
        {
            [PreserveSig]
            int Clone([MarshalAs(UnmanagedType.Interface)] out WbemNative.IWbemContext ppNewCopy);
            [PreserveSig]
            int GetNames([In] int lFlags, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_BSTR)] out string[] pNames);
            [PreserveSig]
            int BeginEnumeration([In] int lFlags);
            [PreserveSig]
            int Next([In] int lFlags, [MarshalAs(UnmanagedType.BStr)] out string pstrName, out object pValue);
            [PreserveSig]
            int EndEnumeration();
            [PreserveSig]
            int SetValue([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In] ref object pValue);
            [PreserveSig]
            int GetValue([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, out object pValue);
            [PreserveSig]
            int DeleteValue([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags);
            [PreserveSig]
            int DeleteAll();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("1005CBCF-E64F-4646-BCD3-3A089D8A84B4")]
        internal interface IWbemDecoupledRegistrar
        {
            [PreserveSig]
            int Register([In] int flags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext context, [In, MarshalAs(UnmanagedType.LPWStr)] string user, [In, MarshalAs(UnmanagedType.LPWStr)] string locale, [In, MarshalAs(UnmanagedType.LPWStr)] string scope, [In, MarshalAs(UnmanagedType.LPWStr)] string registration, [In, MarshalAs(UnmanagedType.IUnknown)] object unknown);
            [PreserveSig]
            int UnRegister();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7C857801-7381-11CF-884D-00AA004B2E24")]
        internal interface IWbemObjectSink
        {
            [PreserveSig]
            int Indicate([In] int lObjectCount, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] WbemNative.IWbemClassObject[] apObjArray);
            [PreserveSig]
            int SetStatus([In] int lFlags, [In, MarshalAs(UnmanagedType.Error)] int hResult, [In, MarshalAs(UnmanagedType.BStr)] string strParam, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemClassObject pObjParam);
        }

        [ComImport, Guid("1BE41572-91DD-11D1-AEB2-00C04FB68820"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IWbemProviderInit
        {
            [PreserveSig]
            int Initialize([In, MarshalAs(UnmanagedType.LPWStr)] string wszUser, [In] int lFlags, [In, MarshalAs(UnmanagedType.LPWStr)] string wszNamespace, [In, MarshalAs(UnmanagedType.LPWStr)] string wszLocale, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemServices pNamespace, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemProviderInitSink pInitSink);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("1BE41571-91DD-11D1-AEB2-00C04FB68820")]
        internal interface IWbemProviderInitSink
        {
            [PreserveSig]
            int SetStatus([In] int lStatus, [In] int lFlags);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("DC12A680-737F-11CF-884D-00AA004B2E24")]
        internal interface IWbemQualifierSet
        {
            [PreserveSig]
            int Get([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In, Out] ref object pVal, [In, Out] ref int plFlavor);
            [PreserveSig]
            int Put([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] ref object pVal, [In] int lFlavor);
            [PreserveSig]
            int Delete([In, MarshalAs(UnmanagedType.LPWStr)] string wszName);
            [PreserveSig]
            int GetNames([In] int lFlags, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_BSTR)] out string[] pNames);
            [PreserveSig]
            int BeginEnumeration([In] int lFlags);
            [PreserveSig]
            int Next([In] int lFlags, [In, Out, MarshalAs(UnmanagedType.BStr)] ref string pstrName, [In, Out] ref object pVal, [In, Out] ref int plFlavor);
            [PreserveSig]
            int EndEnumeration();
        }

        [ComImport, Guid("9556DC99-828C-11CF-A37E-00AA003240C7"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IWbemServices
        {
            [PreserveSig]
            int OpenNamespace([In, MarshalAs(UnmanagedType.BStr)] string strNamespace, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, Out, MarshalAs(UnmanagedType.Interface)] ref WbemNative.IWbemServices ppWorkingNamespace, [In] IntPtr ppCallResult);
            [PreserveSig]
            int CancelAsyncCall([In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemObjectSink pSink);
            [PreserveSig]
            int QueryObjectSink([In] int lFlags, [MarshalAs(UnmanagedType.Interface)] out WbemNative.IWbemObjectSink ppResponseHandler);
            [PreserveSig]
            int GetObject([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, Out, MarshalAs(UnmanagedType.Interface)] ref WbemNative.IWbemClassObject ppObject, [In] IntPtr ppCallResult);
            [PreserveSig]
            int GetObjectAsync([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemObjectSink pResponseHandler);
            [PreserveSig]
            int PutClass([In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemClassObject pObject, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In] IntPtr ppCallResult);
            [PreserveSig]
            int PutClassAsync([In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemClassObject pObject, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemObjectSink pResponseHandler);
            [PreserveSig]
            int DeleteClass([In, MarshalAs(UnmanagedType.BStr)] string strClass, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In] IntPtr ppCallResult);
            [PreserveSig]
            int DeleteClassAsync([In, MarshalAs(UnmanagedType.BStr)] string strClass, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemObjectSink pResponseHandler);
            [PreserveSig]
            int CreateClassEnum([In, MarshalAs(UnmanagedType.BStr)] string strSuperclass, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out WbemNative.IEnumWbemClassObject ppEnum);
            [PreserveSig]
            int CreateClassEnumAsync([In, MarshalAs(UnmanagedType.BStr)] string strSuperclass, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemObjectSink pResponseHandler);
            [PreserveSig]
            int PutInstance([In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemClassObject pInst, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In] IntPtr ppCallResult);
            [PreserveSig]
            int PutInstanceAsync([In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemClassObject pInst, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemObjectSink pResponseHandler);
            [PreserveSig]
            int DeleteInstance([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In] IntPtr ppCallResult);
            [PreserveSig]
            int DeleteInstanceAsync([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemObjectSink pResponseHandler);
            [PreserveSig]
            int CreateInstanceEnum([In, MarshalAs(UnmanagedType.BStr)] string strFilter, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out WbemNative.IEnumWbemClassObject ppEnum);
            [PreserveSig]
            int CreateInstanceEnumAsync([In, MarshalAs(UnmanagedType.BStr)] string strFilter, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemObjectSink pResponseHandler);
            [PreserveSig]
            int ExecQuery([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out WbemNative.IEnumWbemClassObject ppEnum);
            [PreserveSig]
            int ExecQueryAsync([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemObjectSink pResponseHandler);
            [PreserveSig]
            int ExecNotificationQuery([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out WbemNative.IEnumWbemClassObject ppEnum);
            [PreserveSig]
            int ExecNotificationQueryAsync([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemObjectSink pResponseHandler);
            [PreserveSig]
            int ExecMethod([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In, MarshalAs(UnmanagedType.BStr)] string strMethodName, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemClassObject pInParams, [In, Out, MarshalAs(UnmanagedType.Interface)] ref WbemNative.IWbemClassObject ppOutParams, [In] IntPtr ppCallResult);
            [PreserveSig]
            int ExecMethodAsync([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In, MarshalAs(UnmanagedType.BStr)] string strMethodName, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemClassObject pInParams, [In, MarshalAs(UnmanagedType.Interface)] WbemNative.IWbemObjectSink pResponseHandler);
        }

        internal enum tag_WBEM_EXTRA_RETURN_CODES
        {
            WBEM_E_RESOURCE_CONTENTION = -2147209214,
            WBEM_E_RETRY_LATER = -2147209215,
            WBEM_S_INDIRECTLY_UPDATED = 0x43002,
            WBEM_S_INITIALIZED = 0,
            WBEM_S_LIMITED_SERVICE = 0x43001,
            WBEM_S_SUBJECT_TO_SDS = 0x43003
        }

        internal enum tag_WBEM_STATUS_TYPE
        {
            WBEM_STATUS_COMPLETE,
            WBEM_STATUS_REQUIREMENTS,
            WBEM_STATUS_PROGRESS
        }

        [ComImport, Guid("4CFC7932-0F9D-4BEF-9C32-8EA2A6B56FCB")]
        internal class WbemDecoupledRegistrar
        {
        }

        internal enum WbemStatus
        {
            WBEM_E_ACCESS_DENIED = -2147217405,
            WBEM_E_AGGREGATING_BY_OBJECT = -2147217315,
            WBEM_E_ALREADY_EXISTS = -2147217383,
            WBEM_E_AMBIGUOUS_OPERATION = -2147217301,
            WBEM_E_AMENDED_OBJECT = -2147217306,
            WBEM_E_BACKUP_RESTORE_WINMGMT_RUNNING = -2147217312,
            WBEM_E_BUFFER_TOO_SMALL = -2147217348,
            WBEM_E_CALL_CANCELLED = -2147217358,
            WBEM_E_CANNOT_BE_ABSTRACT = -2147217307,
            WBEM_E_CANNOT_BE_KEY = -2147217377,
            WBEM_E_CANNOT_BE_SINGLETON = -2147217364,
            WBEM_E_CANNOT_CHANGE_INDEX_INHERITANCE = -2147217328,
            WBEM_E_CANNOT_CHANGE_KEY_INHERITANCE = -2147217335,
            WBEM_E_CIRCULAR_REFERENCE = -2147217337,
            WBEM_E_CLASS_HAS_CHILDREN = -2147217371,
            WBEM_E_CLASS_HAS_INSTANCES = -2147217370,
            WBEM_E_CLASS_NAME_TOO_WIDE = -2147217292,
            WBEM_E_CLIENT_TOO_SLOW = -2147217305,
            WBEM_E_CONNECTION_FAILED = -2147217295,
            WBEM_E_CRITICAL_ERROR = -2147217398,
            WBEM_E_DATABASE_VER_MISMATCH = -2147217288,
            WBEM_E_ENCRYPTED_CONNECTION_REQUIRED = -2147217273,
            WBEM_E_FAILED = -2147217407,
            WBEM_E_FATAL_TRANSPORT_ERROR = -2147217274,
            WBEM_E_HANDLE_OUT_OF_DATE = -2147217296,
            WBEM_E_ILLEGAL_NULL = -2147217368,
            WBEM_E_ILLEGAL_OPERATION = -2147217378,
            WBEM_E_INCOMPLETE_CLASS = -2147217376,
            WBEM_E_INITIALIZATION_FAILURE = -2147217388,
            WBEM_E_INVALID_ASSOCIATION = -2147217302,
            WBEM_E_INVALID_CIM_TYPE = -2147217363,
            WBEM_E_INVALID_CLASS = -2147217392,
            WBEM_E_INVALID_CONTEXT = -2147217401,
            WBEM_E_INVALID_DUPLICATE_PARAMETER = -2147217341,
            WBEM_E_INVALID_FLAVOR = -2147217338,
            WBEM_E_INVALID_HANDLE_REQUEST = -2147217294,
            WBEM_E_INVALID_LOCALE = -2147217280,
            WBEM_E_INVALID_METHOD = -2147217362,
            WBEM_E_INVALID_METHOD_PARAMETERS = -2147217361,
            WBEM_E_INVALID_NAMESPACE = -2147217394,
            WBEM_E_INVALID_OBJECT = -2147217393,
            WBEM_E_INVALID_OBJECT_PATH = -2147217350,
            WBEM_E_INVALID_OPERATION = -2147217386,
            WBEM_E_INVALID_OPERATOR = -2147217309,
            WBEM_E_INVALID_PARAMETER = -2147217400,
            WBEM_E_INVALID_PARAMETER_ID = -2147217353,
            WBEM_E_INVALID_PROPERTY = -2147217359,
            WBEM_E_INVALID_PROPERTY_TYPE = -2147217366,
            WBEM_E_INVALID_PROVIDER_REGISTRATION = -2147217390,
            WBEM_E_INVALID_QUALIFIER = -2147217342,
            WBEM_E_INVALID_QUALIFIER_TYPE = -2147217367,
            WBEM_E_INVALID_QUERY = -2147217385,
            WBEM_E_INVALID_QUERY_TYPE = -2147217384,
            WBEM_E_INVALID_STREAM = -2147217397,
            WBEM_E_INVALID_SUPERCLASS = -2147217395,
            WBEM_E_INVALID_SYNTAX = -2147217375,
            WBEM_E_LOCAL_CREDENTIALS = -2147217308,
            WBEM_E_MARSHAL_INVALID_SIGNATURE = -2147217343,
            WBEM_E_MARSHAL_VERSION_MISMATCH = -2147217344,
            WBEM_E_METHOD_DISABLED = -2147217322,
            WBEM_E_METHOD_NAME_TOO_WIDE = -2147217291,
            WBEM_E_METHOD_NOT_IMPLEMENTED = -2147217323,
            WBEM_E_MISSING_AGGREGATION_LIST = -2147217317,
            WBEM_E_MISSING_GROUP_WITHIN = -2147217318,
            WBEM_E_MISSING_PARAMETER_ID = -2147217354,
            WBEM_E_NO_KEY = -2147217271,
            WBEM_E_NO_SCHEMA = -2147217277,
            WBEM_E_NONCONSECUTIVE_PARAMETER_IDS = -2147217352,
            WBEM_E_NONDECORATED_OBJECT = -2147217374,
            WBEM_E_NOT_AVAILABLE = -2147217399,
            WBEM_E_NOT_EVENT_CLASS = -2147217319,
            WBEM_E_NOT_FOUND = -2147217406,
            WBEM_E_NOT_SUPPORTED = -2147217396,
            WBEM_E_NULL_SECURITY_DESCRIPTOR = -2147217304,
            WBEM_E_OUT_OF_DISK_SPACE = -2147217349,
            WBEM_E_OUT_OF_MEMORY = -2147217402,
            WBEM_E_OVERRIDE_NOT_ALLOWED = -2147217382,
            WBEM_E_PARAMETER_ID_ON_RETVAL = -2147217351,
            WBEM_E_PRIVILEGE_NOT_HELD = -2147217310,
            WBEM_E_PROPAGATED_METHOD = -2147217356,
            WBEM_E_PROPAGATED_PROPERTY = -2147217380,
            WBEM_E_PROPAGATED_QUALIFIER = -2147217381,
            WBEM_E_PROPERTY_NAME_TOO_WIDE = -2147217293,
            WBEM_E_PROPERTY_NOT_AN_OBJECT = -2147217316,
            WBEM_E_PROVIDER_ALREADY_REGISTERED = -2147217276,
            WBEM_E_PROVIDER_FAILURE = -2147217404,
            WBEM_E_PROVIDER_LOAD_FAILURE = -2147217389,
            WBEM_E_PROVIDER_NOT_CAPABLE = -2147217372,
            WBEM_E_PROVIDER_NOT_FOUND = -2147217391,
            WBEM_E_PROVIDER_NOT_REGISTERED = -2147217275,
            WBEM_E_PROVIDER_SUSPENDED = -2147217279,
            WBEM_E_PROVIDER_TIMED_OUT = -2147217272,
            WBEM_E_QUALIFIER_NAME_TOO_WIDE = -2147217290,
            WBEM_E_QUERY_NOT_IMPLEMENTED = -2147217369,
            WBEM_E_QUEUE_OVERFLOW = -2147217311,
            WBEM_E_QUOTA_VIOLATION = -2147217300,
            WBEM_E_READ_ONLY = -2147217373,
            WBEM_E_REFRESHER_BUSY = -2147217321,
            WBEM_E_RERUN_COMMAND = -2147217289,
            WBEM_E_RESERVED_001 = -2147217299,
            WBEM_E_RESERVED_002 = -2147217298,
            WBEM_E_SERVER_TOO_BUSY = -2147217339,
            WBEM_E_SHUTTING_DOWN = -2147217357,
            WBEM_E_SYNCHRONIZATION_REQUIRED = -2147217278,
            WBEM_E_SYSTEM_PROPERTY = -2147217360,
            WBEM_E_TIMED_OUT = -2147217303,
            WBEM_E_TOO_MANY_PROPERTIES = -2147217327,
            WBEM_E_TOO_MUCH_DATA = -2147217340,
            WBEM_E_TRANSPORT_FAILURE = -2147217387,
            WBEM_E_TYPE_MISMATCH = -2147217403,
            WBEM_E_UNEXPECTED = -2147217379,
            WBEM_E_UNINTERPRETABLE_PROVIDER_QUERY = -2147217313,
            WBEM_E_UNKNOWN_OBJECT_TYPE = -2147217346,
            WBEM_E_UNKNOWN_PACKET_TYPE = -2147217345,
            WBEM_E_UNPARSABLE_QUERY = -2147217320,
            WBEM_E_UNSUPPORTED_CLASS_UPDATE = -2147217336,
            WBEM_E_UNSUPPORTED_LOCALE = -2147217297,
            WBEM_E_UNSUPPORTED_PARAMETER = -2147217355,
            WBEM_E_UNSUPPORTED_PUT_EXTENSION = -2147217347,
            WBEM_E_UPDATE_OVERRIDE_NOT_ALLOWED = -2147217325,
            WBEM_E_UPDATE_PROPAGATED_METHOD = -2147217324,
            WBEM_E_UPDATE_TYPE_MISMATCH = -2147217326,
            WBEM_E_VALUE_OUT_OF_RANGE = -2147217365,
            WBEM_E_VETO_DELETE = -2147217287,
            WBEM_E_VETO_PUT = -2147217286,
            WBEM_NO_ERROR = 0,
            WBEM_S_ACCESS_DENIED = 0x40009,
            WBEM_S_ALREADY_EXISTS = 0x40001,
            WBEM_S_DIFFERENT = 0x40003,
            WBEM_S_DUPLICATE_OBJECTS = 0x40008,
            WBEM_S_FALSE = 1,
            WBEM_S_NO_ERROR = 0,
            WBEM_S_NO_MORE_DATA = 0x40005,
            WBEM_S_NO_POSTHOOK = 0x40011,
            WBEM_S_OPERATION_CANCELLED = 0x40006,
            WBEM_S_PARTIAL_RESULTS = 0x40010,
            WBEM_S_PENDING = 0x40007,
            WBEM_S_POSTHOOK_WITH_BOTH = 0x40012,
            WBEM_S_POSTHOOK_WITH_NEW = 0x40013,
            WBEM_S_POSTHOOK_WITH_OLD = 0x40015,
            WBEM_S_POSTHOOK_WITH_STATUS = 0x40014,
            WBEM_S_REDO_PREHOOK_WITH_ORIGINAL_OBJECT = 0x40016,
            WBEM_S_RESET_TO_DEFAULT = 0x40002,
            WBEM_S_SAME = 0,
            WBEM_S_SOURCE_NOT_AVAILABLE = 0x40017,
            WBEM_S_TIMEDOUT = 0x40004,
            WBEMESS_E_REGISTRATION_TOO_BROAD = -2147213311,
            WBEMESS_E_REGISTRATION_TOO_PRECISE = -2147213310,
            WBEMMOF_E_ALIASES_IN_EMBEDDED = -2147205089,
            WBEMMOF_E_CIMTYPE_QUALIFIER = -2147205094,
            WBEMMOF_E_DUPLICATE_PROPERTY = -2147205093,
            WBEMMOF_E_DUPLICATE_QUALIFIER = -2147205087,
            WBEMMOF_E_ERROR_CREATING_TEMP_FILE = -2147205073,
            WBEMMOF_E_ERROR_INVALID_INCLUDE_FILE = -2147205072,
            WBEMMOF_E_EXPECTED_ALIAS_NAME = -2147205098,
            WBEMMOF_E_EXPECTED_BRACE_OR_BAD_TYPE = -2147205079,
            WBEMMOF_E_EXPECTED_CLASS_NAME = -2147205100,
            WBEMMOF_E_EXPECTED_CLOSE_BRACE = -2147205116,
            WBEMMOF_E_EXPECTED_CLOSE_BRACKET = -2147205115,
            WBEMMOF_E_EXPECTED_CLOSE_PAREN = -2147205114,
            WBEMMOF_E_EXPECTED_DOLLAR = -2147205095,
            WBEMMOF_E_EXPECTED_FLAVOR_TYPE = -2147205086,
            WBEMMOF_E_EXPECTED_OPEN_BRACE = -2147205117,
            WBEMMOF_E_EXPECTED_OPEN_PAREN = -2147205111,
            WBEMMOF_E_EXPECTED_PROPERTY_NAME = -2147205108,
            WBEMMOF_E_EXPECTED_QUALIFIER_NAME = -2147205119,
            WBEMMOF_E_EXPECTED_SEMI = -2147205118,
            WBEMMOF_E_EXPECTED_TYPE_IDENTIFIER = -2147205112,
            WBEMMOF_E_ILLEGAL_CONSTANT_VALUE = -2147205113,
            WBEMMOF_E_INCOMPATIBLE_FLAVOR_TYPES = -2147205085,
            WBEMMOF_E_INCOMPATIBLE_FLAVOR_TYPES2 = -2147205083,
            WBEMMOF_E_INVALID_AMENDMENT_SYNTAX = -2147205104,
            WBEMMOF_E_INVALID_CLASS_DECLARATION = -2147205097,
            WBEMMOF_E_INVALID_DELETECLASS_SYNTAX = -2147205071,
            WBEMMOF_E_INVALID_DELETEINSTANCE_SYNTAX = -2147205076,
            WBEMMOF_E_INVALID_DUPLICATE_AMENDMENT = -2147205103,
            WBEMMOF_E_INVALID_FILE = -2147205090,
            WBEMMOF_E_INVALID_FLAGS_SYNTAX = -2147205080,
            WBEMMOF_E_INVALID_INSTANCE_DECLARATION = -2147205096,
            WBEMMOF_E_INVALID_NAMESPACE_SPECIFICATION = -2147205092,
            WBEMMOF_E_INVALID_NAMESPACE_SYNTAX = -2147205101,
            WBEMMOF_E_INVALID_PRAGMA = -2147205102,
            WBEMMOF_E_INVALID_QUALIFIER_SYNTAX = -2147205075,
            WBEMMOF_E_MULTIPLE_ALIASES = -2147205084,
            WBEMMOF_E_MUST_BE_IN_OR_OUT = -2147205081,
            WBEMMOF_E_NO_ARRAYS_RETURNED = -2147205082,
            WBEMMOF_E_NULL_ARRAY_ELEM = -2147205088,
            WBEMMOF_E_OUT_OF_RANGE = -2147205091,
            WBEMMOF_E_QUALIFIER_USED_OUTSIDE_SCOPE = -2147205074,
            WBEMMOF_E_TYPE_MISMATCH = -2147205099,
            WBEMMOF_E_TYPEDEF_NOT_SUPPORTED = -2147205107,
            WBEMMOF_E_UNEXPECTED_ALIAS = -2147205106,
            WBEMMOF_E_UNEXPECTED_ARRAY_INIT = -2147205105,
            WBEMMOF_E_UNRECOGNIZED_TOKEN = -2147205110,
            WBEMMOF_E_UNRECOGNIZED_TYPE = -2147205109,
            WBEMMOF_E_UNSUPPORTED_CIMV22_DATA_TYPE = -2147205077,
            WBEMMOF_E_UNSUPPORTED_CIMV22_QUAL_VALUE = -2147205078
        }
    }
}

