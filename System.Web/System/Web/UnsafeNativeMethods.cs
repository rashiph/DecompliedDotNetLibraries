namespace System.Web
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;

    [ComVisible(false), SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        public const int DACL_SECURITY_INFORMATION = 4;
        internal const int DELETE = 0x10000;
        public const int ERROR_NO_TOKEN = 0x3f0;
        internal const int FILE_ATTRIBUTE_ARCHIVE = 0x20;
        internal const int FILE_ATTRIBUTE_COMPRESSED = 0x800;
        internal const int FILE_ATTRIBUTE_DEVICE = 0x40;
        internal const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        internal const int FILE_ATTRIBUTE_ENCRYPTED = 0x4000;
        internal const int FILE_ATTRIBUTE_HIDDEN = 2;
        internal const int FILE_ATTRIBUTE_NORMAL = 0x80;
        internal const int FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x2000;
        internal const int FILE_ATTRIBUTE_OFFLINE = 0x1000;
        internal const int FILE_ATTRIBUTE_READONLY = 1;
        internal const int FILE_ATTRIBUTE_REPARSE_POINT = 0x400;
        internal const int FILE_ATTRIBUTE_SPARSE_FILE = 0x200;
        internal const int FILE_ATTRIBUTE_SYSTEM = 4;
        internal const int FILE_ATTRIBUTE_TEMPORARY = 0x100;
        internal const int FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;
        internal const int FILE_FLAG_DELETE_ON_CLOSE = 0x4000000;
        internal const int FILE_FLAG_NO_BUFFERING = 0x20000000;
        internal const int FILE_FLAG_OVERLAPPED = 0x40000000;
        internal const int FILE_FLAG_POSIX_SEMANTICS = 0x1000000;
        internal const int FILE_FLAG_RANDOM_ACCESS = 0x10000000;
        internal const int FILE_FLAG_SEQUENTIAL_SCAN = 0x8000000;
        internal const int FILE_FLAG_WRITE_THROUGH = -2147483648;
        internal const uint FILE_NOTIFY_CHANGE_ATTRIBUTES = 4;
        internal const uint FILE_NOTIFY_CHANGE_CREATION = 0x40;
        internal const uint FILE_NOTIFY_CHANGE_DIR_NAME = 2;
        internal const uint FILE_NOTIFY_CHANGE_FILE_NAME = 1;
        internal const uint FILE_NOTIFY_CHANGE_LAST_ACCESS = 0x20;
        internal const uint FILE_NOTIFY_CHANGE_LAST_WRITE = 0x10;
        internal const uint FILE_NOTIFY_CHANGE_SECURITY = 0x100;
        internal const uint FILE_NOTIFY_CHANGE_SIZE = 8;
        internal const int FILE_SHARE_DELETE = 4;
        internal const int FILE_SHARE_READ = 1;
        internal const int FILE_SHARE_WRITE = 2;
        internal const int GENERIC_READ = -2147483648;
        internal const int GetFileExInfoStandard = 0;
        public const int GROUP_SECURITY_INFORMATION = 2;
        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        internal const int OPEN_ALWAYS = 4;
        internal const int OPEN_EXISTING = 3;
        public const int OWNER_SECURITY_INFORMATION = 1;
        internal const uint RDCW_FILTER_DIR_RENAMES = 2;
        internal const uint RDCW_FILTER_FILE_AND_DIR_CHANGES = 0x15b;
        internal const uint RDCW_FILTER_FILE_CHANGES = 0x159;
        internal const int READ_CONTROL = 0x20000;
        public const int RESTRICT_BIN = 1;
        public const int SACL_SECURITY_INFORMATION = 8;
        internal const int SPECIFIC_RIGHTS_ALL = 0xffff;
        internal const int STANDARD_RIGHTS_ALL = 0x1f0000;
        internal const int STANDARD_RIGHTS_EXECUTE = 0x20000;
        internal const int STANDARD_RIGHTS_READ = 0x20000;
        internal const int STANDARD_RIGHTS_REQUIRED = 0xf0000;
        internal const int STANDARD_RIGHTS_WRITE = 0x20000;
        internal const int StateProtocolFlagUninitialized = 1;
        internal const int SYNCHRONIZE = 0x100000;
        public const int TOKEN_ALL_ACCESS = 0xf01ff;
        public const int TOKEN_EXECUTE = 0x20000;
        public const int TOKEN_IMPERSONATE = 4;
        public const int TOKEN_READ = 0x20008;
        internal const int WRITE_DAC = 0x40000;
        internal const int WRITE_OWNER = 0x80000;

        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern void AppDomainRestart(string appId);
        [DllImport("webengine4.dll")]
        internal static extern int AspCompatIsApartmentComponent([MarshalAs(UnmanagedType.Interface)] object obj);
        [DllImport("webengine4.dll")]
        internal static extern int AspCompatOnPageEnd();
        [DllImport("webengine4.dll")]
        internal static extern int AspCompatOnPageStart([MarshalAs(UnmanagedType.Interface)] object obj);
        [DllImport("webengine4.dll")]
        internal static extern int AspCompatProcessRequest(AspCompatCallback callback, [MarshalAs(UnmanagedType.Interface)] object context, bool sharedActivity, int activityHash);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int AttachDebugger(string clsId, string sessId, IntPtr userToken);
        [DllImport("webengine4.dll")]
        internal static extern IntPtr BufferPoolGetBuffer(IntPtr pool);
        [DllImport("webengine4.dll")]
        internal static extern IntPtr BufferPoolGetPool(int bufferSize, int maxFreeListCount);
        [DllImport("webengine4.dll")]
        internal static extern void BufferPoolReleaseBuffer(IntPtr buffer);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int ChangeAccessToKeyContainer(string containerName, string accountName, string csp, int options);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool CloseHandle(IntPtr handle);
        [DllImport("ole32.dll", CharSet=CharSet.Unicode)]
        internal static extern int CoCreateInstanceEx(ref Guid clsid, IntPtr pUnkOuter, int dwClsContext, [In, Out] COSERVERINFO srv, int num, [In, Out] MULTI_QI[] amqi);
        [DllImport("ole32.dll", CharSet=CharSet.Unicode)]
        internal static extern int CoCreateInstanceEx(ref Guid clsid, IntPtr pUnkOuter, int dwClsContext, [In, Out] COSERVERINFO_X64 srv, int num, [In, Out] MULTI_QI_X64[] amqi);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int ConvertStringSidToSid(string stringSid, out IntPtr pSid);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int CookieAuthConstructTicket(byte[] pData, int iDataLen, string szName, string szData, string szPath, byte[] pBytes, long[] pDates);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int CookieAuthParseTicket(byte[] pData, int iDataLen, StringBuilder szName, int iNameLen, StringBuilder szData, int iUserDataLen, StringBuilder szPath, int iPathLen, byte[] pBytes, long[] pDates);
        [DllImport("ole32.dll", CharSet=CharSet.Unicode)]
        internal static extern int CoSetProxyBlanket(IntPtr pProxy, RpcAuthent authent, RpcAuthor author, string serverprinc, RpcLevel level, RpcImpers impers, IntPtr ciptr, int dwCapabilities);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern IntPtr CreateUserToken(string name, string password, int fImpersonationToken, StringBuilder strError, int iErrorSize);
        [DllImport("clr.dll", CharSet=CharSet.Unicode)]
        internal static extern int DeleteShadowCache(string pwzCachePath, string pwzAppName);
        [DllImport("webengine4.dll")]
        internal static extern void DirMonClose(HandleRef dirMon);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int DirMonOpen(string dir, string appId, bool watchSubtree, uint notifyFilter, NativeFileChangeNotification callback, out IntPtr pCompletion);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int DoesKeyContainerExist(string containerName, string provider, int useMachineContainer);
        [DllImport("webengine4.dll", CharSet=CharSet.Ansi)]
        internal static extern int EcbAppendLogParameter(IntPtr pECB, string logParam);
        [DllImport("webengine4.dll")]
        internal static extern int EcbCallISAPI(IntPtr pECB, CallISAPIFunc iFunction, byte[] bufferIn, int sizeIn, byte[] bufferOut, int sizeOut);
        [DllImport("webengine4.dll")]
        internal static extern int EcbCloseConnection(IntPtr pECB);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int EcbEmitSimpleTrace(IntPtr pECB, int type, string eventData);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int EcbEmitWebEventTrace(IntPtr pECB, int webEventType, int fieldCount, string[] fieldNames, int[] fieldTypes, string[] fieldData);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int EcbExecuteUrlUnicode(IntPtr pECB, string url, string method, string childHeaders, bool sendHeaders, bool addUserIndo, IntPtr token, string name, string authType, IntPtr pEntity, ISAPIAsyncCompletionCallback asyncCompletionCallback);
        [DllImport("webengine4.dll")]
        internal static extern int EcbFlushCore(IntPtr pECB, byte[] status, byte[] header, int keepConnected, int totalBodySize, int numBodyFragments, IntPtr[] bodyFragments, int[] bodyFragmentLengths, int doneWithSession, int finalStatus, int kernelCache, int async, ISAPIAsyncCompletionCallback asyncCompletionCallback);
        [DllImport("webengine4.dll")]
        internal static extern void EcbFreeExecUrlEntityInfo(IntPtr pEntity);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetAdditionalPostedContent(IntPtr pECB, byte[] bytes, int offset, int bufferSize);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetBasics(IntPtr pECB, byte[] buffer, int size, int[] contentInfo);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetBasicsContentInfo(IntPtr pECB, int[] contentInfo);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetChannelBindingToken(IntPtr pECB, out IntPtr token, out int tokenSize);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetClientCertificate(IntPtr pECB, byte[] buffer, int size, int[] pInts, long[] pDates);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetExecUrlEntityInfo(int entityLength, byte[] entity, out IntPtr ppEntity);
        [DllImport("webengine4.dll")]
        internal static extern IntPtr EcbGetImpersonationToken(IntPtr pECB, IntPtr processHandle);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetPreloadedPostedContent(IntPtr pECB, byte[] bytes, int offset, int bufferSize);
        [DllImport("webengine4.dll", CharSet=CharSet.Ansi)]
        internal static extern int EcbGetQueryString(IntPtr pECB, int encode, StringBuilder buffer, int size);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetQueryStringRawBytes(IntPtr pECB, byte[] buffer, int size);
        [DllImport("webengine4.dll", CharSet=CharSet.Ansi)]
        internal static extern int EcbGetServerVariable(IntPtr pECB, string name, byte[] buffer, int size);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetServerVariableByIndex(IntPtr pECB, int nameIndex, byte[] buffer, int size);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetTraceContextId(IntPtr pECB, out Guid traceContextId);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetTraceFlags(IntPtr pECB, int[] contentInfo);
        [DllImport("webengine4.dll", CharSet=CharSet.Ansi)]
        internal static extern int EcbGetUnicodeServerVariable(IntPtr pECB, string name, IntPtr buffer, int size);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetUnicodeServerVariableByIndex(IntPtr pECB, int nameIndex, IntPtr buffer, int size);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetUnicodeServerVariables(IntPtr pECB, IntPtr buffer, int bufferSizeInChars, int[] serverVarLengths, int serverVarCount, int startIndex, ref int requiredSize);
        [DllImport("webengine4.dll")]
        internal static extern int EcbGetVersion(IntPtr pECB);
        [DllImport("webengine4.dll")]
        internal static extern IntPtr EcbGetVirtualPathToken(IntPtr pECB, IntPtr processHandle);
        [DllImport("webengine4.dll")]
        internal static extern int EcbIsClientConnected(IntPtr pECB);
        [DllImport("webengine4.dll", CharSet=CharSet.Ansi)]
        internal static extern int EcbMapUrlToPath(IntPtr pECB, string url, byte[] buffer, int size);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool FindClose(IntPtr hndFindFile);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr FindFirstFile(string pFileName, out WIN32_FIND_DATA pFindFileData);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool FindNextFile(IntPtr hndFindFile, out WIN32_FIND_DATA pFindFileData);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);
        [DllImport("webengine4.dll")]
        internal static extern void FreeFileSecurityDescriptor(IntPtr securityDesciptor);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("clr.dll", CharSet=CharSet.Unicode)]
        internal static extern int GetCachePath(int dwCacheFlags, StringBuilder pwzCachePath, ref int pcchPath);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern int GetComputerName(StringBuilder nameBuffer, ref int bufferSize);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int GetCredentialFromRegistry(string strRegKey, StringBuilder buffer, int size);
        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetCurrentThread();
        [DllImport("webengine4.dll")]
        internal static extern void GetDirMonConfiguration(out int FCNMode);
        [DllImport("webengine4.dll")]
        internal static extern IntPtr GetEcb(IntPtr pHttpCompletion);
        [DllImport("webengine4.dll")]
        internal static extern void GetEtwValues(out int level, out int flags);
        [DllImport("aspnet_filter.dll")]
        internal static extern IntPtr GetExtensionlessUrlAppendage();
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool GetFileAttributesEx(string name, int fileInfoLevel, out WIN32_FILE_ATTRIBUTE_DATA data);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr GetFileHandleForTransmitFile(string strFile);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int GetFileSecurity(string filename, int requestedInformation, byte[] securityDescriptor, int length, ref int lengthNeeded);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern IntPtr GetFileSecurityDescriptor(string strFile);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int GetGroupsForUser(IntPtr token, StringBuilder allGroups, int allGrpSize, StringBuilder error, int errorSize);
        [DllImport("webengine4.dll")]
        internal static extern int GetHMACSHA1Hash(byte[] data1, int dataOffset1, int dataSize1, byte[] data2, int dataSize2, byte[] innerKey, int innerKeySize, byte[] outerKey, int outerKeySize, byte[] hash, int hashSize);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern int GetModuleFileName(IntPtr module, StringBuilder filename, int size);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern IntPtr GetModuleHandle(string moduleName);
        [DllImport("webengine4.dll")]
        internal static extern int GetPrivateBytesIIS6(out long privatePageCount, bool nocache);
        [DllImport("kernel32.dll")]
        internal static extern int GetProcessAffinityMask(IntPtr handle, out IntPtr processAffinityMask, out IntPtr systemAffinityMask);
        [DllImport("webengine4.dll")]
        internal static extern int GetProcessMemoryInformation(uint pid, out uint privatePageCount, out uint peakPagefileUsage, bool nocache);
        [DllImport("webengine4.dll")]
        internal static extern int GetSHA1Hash(byte[] data, int dataSize, byte[] hash, int hashSize);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern void GetSystemInfo(out SYSTEM_INFO si);
        [DllImport("webengine4.dll")]
        internal static extern int GetW3WPMemoryLimitInKB();
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern int GlobalMemoryStatusEx(ref MEMORYSTATUSEX memoryStatusEx);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int GrowFileNotificationBuffer(string appId, bool fWatchSubtree);
        [DllImport("webengine4.dll")]
        internal static extern void InitializeHealthMonitor(int deadlockIntervalSeconds, int requestQueueLimit);
        [DllImport("webengine4.dll")]
        internal static extern void InitializeLibrary(bool reduceMaxThreads);
        [DllImport("webengine4.dll")]
        internal static extern int InitializeWmiManager();
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern IntPtr InstrumentedMutexCreate(string name);
        [DllImport("webengine4.dll")]
        internal static extern void InstrumentedMutexDelete(HandleRef mutex);
        [DllImport("webengine4.dll")]
        internal static extern int InstrumentedMutexGetLock(HandleRef mutex, int timeout);
        [DllImport("webengine4.dll")]
        internal static extern int InstrumentedMutexReleaseLock(HandleRef mutex);
        [DllImport("webengine4.dll")]
        internal static extern void InstrumentedMutexSetState(HandleRef mutex, int state);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern void InvalidateKernelCache(string key);
        [DllImport("webengine4.dll")]
        internal static extern int IsAccessToFileAllowed(IntPtr securityDesciptor, IntPtr iThreadToken, int iAccess);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int IsapiAppHostGetAppPath(string aboPath, StringBuilder buffer, int size);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int IsapiAppHostGetNextVirtualSubdir(string aboPath, bool inApp, ref int index, StringBuilder sb, int size);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int IsapiAppHostGetSiteId(string site, StringBuilder buffer, int size);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int IsapiAppHostGetSiteName(string appId, StringBuilder buffer, int size);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int IsapiAppHostGetUncUser(string appId, StringBuilder usernameBuffer, int usernameSize, StringBuilder passwordBuffer, int passwordSize);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int IsapiAppHostMapPath(string appId, string virtualPath, StringBuilder buffer, int size);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int IsUserInRole(IntPtr token, string rolename, StringBuilder error, int errorSize);
        [DllImport("webengine4.dll", SetLastError=true)]
        internal static extern bool IsValidResource(IntPtr hModule, IntPtr ip, int size);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr LoadLibrary(string libFilename);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        public static extern IntPtr LocalFree(IntPtr pMem);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr LockResource(IntPtr hResData);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int LogonUser(string username, string domain, string password, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern void LogWebeventProviderFailure(string appUrl, string providerName, string exception);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int LookupAccountSid(string systemName, IntPtr pSid, StringBuilder szName, ref int nameSize, StringBuilder szDomain, ref int domainSize, ref int eUse);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi)]
        internal static extern int lstrlenA(IntPtr ptr);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern int lstrlenW(IntPtr ptr);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool MoveFileEx(string oldFilename, string newFilename, uint flags);
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern int OpenThreadToken(IntPtr thread, int access, bool openAsSelf, ref IntPtr hToken);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportAuthURL(IntPtr iPassport, string szReturnURL, int iTimeWindow, int fForceLogin, string szCOBrandArgs, int iLangID, string strNameSpace, int iKPP, int iUseSecureAuth, StringBuilder szAuthVal, int iAuthValSize);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportAuthURL2(IntPtr iPassport, string szReturnURL, int iTimeWindow, int fForceLogin, string szCOBrandArgs, int iLangID, string strNameSpace, int iKPP, int iUseSecureAuth, StringBuilder szAuthVal, int iAuthValSize);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportCreate(string szQueryStrT, string szQueryStrP, string szAuthCookie, string szProfCookie, string szProfCCookie, StringBuilder szAuthCookieRet, StringBuilder szProfCookieRet, int iRetBufSize, ref IntPtr passportManager);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportCreateHttpRaw(string szRequestLine, string szHeaders, int fSecure, StringBuilder szBufOut, int dwRetBufSize, ref IntPtr passportManager);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportCrypt(int iFunctionID, string szSrc, StringBuilder szDest, int iDestLength);
        [DllImport("webengine4.dll")]
        internal static extern int PassportCryptIsValid();
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportCryptPut(int iFunctionID, string szSrc);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern void PassportDestroy(IntPtr iPassport);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportDomainFromMemberName(IntPtr iPassport, string szDomain, StringBuilder szMember, int iMemberSize);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportGetCurrentConfig(IntPtr pManager, string szAttr, out object pReturn);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportGetDomainAttribute(IntPtr iPassport, string szAttributeName, int iLCID, string szDomain, StringBuilder szValue, int iValueSize);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportGetError(IntPtr iPassport);
        [DllImport("webengine4.dll")]
        internal static extern int PassportGetFromNetworkServer(IntPtr iPassport);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportGetHasSavedPassword(IntPtr iPassport);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportGetLoginChallenge(IntPtr pManager, string szRetURL, int iTimeWindow, int fForceLogin, string szCOBrandArgs, int iLangID, string strNameSpace, int iKPP, int iUseSecureAuth, object vExtraParams, StringBuilder szOut, int iOutSize);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportGetOption(IntPtr pManager, string szOption, out object vOut);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportGetProfile(IntPtr iPassport, string szProfile, out object rOut);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportGetTicketAge(IntPtr iPassport);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportGetTimeSinceSignIn(IntPtr iPassport);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportHasConsent(IntPtr iPassport, int iFullConsent, int iNeedBirthdate);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportHasFlag(IntPtr iPassport, int iFlagMask);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportHasProfile(IntPtr iPassport, string szProfile);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportHasTicket(IntPtr iPassport);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportHexPUID(IntPtr pManager, StringBuilder szOut, int iOutSize);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportIsAuthenticated(IntPtr iPassport, int iTimeWindow, int fForceLogin, int iUseSecureAuth);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportLogoTag(IntPtr iPassport, string szRetURL, int iTimeWindow, int fForceLogin, string szCOBrandArgs, int iLangID, int fSecure, string strNameSpace, int iKPP, int iUseSecureAuth, StringBuilder szValue, int iValueSize);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportLogoTag2(IntPtr iPassport, string szRetURL, int iTimeWindow, int fForceLogin, string szCOBrandArgs, int iLangID, int fSecure, string strNameSpace, int iKPP, int iUseSecureAuth, StringBuilder szValue, int iValueSize);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportLogoutURL(IntPtr pManager, string szReturnURL, string szCOBrandArgs, int iLangID, string strDomain, int iUseSecureAuth, StringBuilder szAuthVal, int iAuthValSize);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportSetOption(IntPtr pManager, string szOption, object vOut);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int PassportTicket(IntPtr pManager, string szAttr, out object pReturn);
        [DllImport("webengine4.dll")]
        internal static extern int PassportVersion();
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("webengine4.dll")]
        internal static extern void PerfCloseAppCounters(IntPtr pCounters);
        [DllImport("webengine4.dll")]
        internal static extern void PerfCounterInitialize();
        [DllImport("webengine4.dll")]
        internal static extern void PerfDecrementCounter(IntPtr pCounters, int number);
        [DllImport("webengine4.dll")]
        internal static extern int PerfGetCounter(IntPtr pCounters, int number);
        [DllImport("webengine4.dll")]
        internal static extern void PerfIncrementCounter(IntPtr pCounters, int number);
        [DllImport("webengine4.dll")]
        internal static extern void PerfIncrementCounterEx(IntPtr pCounters, int number, int increment);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern PerfInstanceDataHandle PerfOpenAppCounters(string AppName);
        [DllImport("webengine4.dll")]
        internal static extern IntPtr PerfOpenGlobalCounters();
        [DllImport("webengine4.dll")]
        internal static extern IntPtr PerfOpenStateCounters();
        [DllImport("webengine4.dll")]
        internal static extern void PerfSetCounter(IntPtr pCounters, int number, int increment);
        [DllImport("aspnet_wp.exe", CharSet=CharSet.Ansi)]
        internal static extern int PMAppendLogParameter(IntPtr pMsg, string logParam);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMCallISAPI(IntPtr pECB, CallISAPIFunc iFunction, byte[] bufferIn, int sizeIn, byte[] bufferOut, int sizeOut);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMCloseConnection(IntPtr pMsg);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMEmptyResponse(IntPtr pMsg);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMFlushCore(IntPtr pMsg, byte[] status, byte[] header, int keepConnected, int totalBodySize, int bodyFragmentsOffset, int numBodyFragments, IntPtr[] bodyFragments, int[] bodyFragmentLengths, int doneWithSession, int finalStatus);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMGetAdditionalPostedContent(IntPtr pMsg, byte[] bytes, int offset, int bufferSize);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMGetAllServerVariables(IntPtr pMsg, byte[] buffer, int size);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMGetBasics(IntPtr pMsg, byte[] buffer, int size, int[] contentInfo);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMGetClientCertificate(IntPtr pMsg, byte[] buffer, int size, int[] pInts, long[] pDates);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMGetCurrentProcessInfo(ref int dwReqExecuted, ref int dwReqExecuting, ref int dwPeakMemoryUsed, ref long tmCreateTime, ref int pid);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMGetHistoryTable(int iRows, int[] dwPIDArr, int[] dwReqExecuted, int[] dwReqPending, int[] dwReqExecuting, int[] dwReasonForDeath, int[] dwPeakMemoryUsed, long[] tmCreateTime, long[] tmDeathTime);
        [DllImport("aspnet_wp.exe")]
        internal static extern IntPtr PMGetImpersonationToken(IntPtr pMsg);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMGetMemoryLimitInMB();
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMGetPreloadedPostedContent(IntPtr pMsg, byte[] bytes, int offset, int bufferSize);
        [DllImport("aspnet_wp.exe", CharSet=CharSet.Ansi)]
        internal static extern int PMGetQueryString(IntPtr pMsg, int encode, StringBuilder buffer, int size);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMGetQueryStringRawBytes(IntPtr pMsg, byte[] buffer, int size);
        [DllImport("aspnet_wp.exe")]
        internal static extern long PMGetStartTimeStamp(IntPtr pMsg);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMGetTraceContextId(IntPtr pMsg, out Guid traceContextId);
        [DllImport("aspnet_wp.exe")]
        internal static extern IntPtr PMGetVirtualPathToken(IntPtr pMsg);
        [DllImport("aspnet_wp.exe")]
        internal static extern int PMIsClientConnected(IntPtr pMsg);
        [DllImport("aspnet_wp.exe", CharSet=CharSet.Ansi)]
        internal static extern int PMMapUrlToPath(IntPtr pMsg, string url, byte[] buffer, int size);
        [DllImport("aspnet_wp.exe", CharSet=CharSet.Unicode)]
        internal static extern void PMTraceRaiseEvent(int eventType, IntPtr pMsg, string data1, string data2, string data3, string data4);
        [DllImport("webengine4.dll")]
        internal static extern int PostThreadPoolWorkItem(WorkItemCallback callback);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int RaiseEventlogEvent(int eventType, string[] dataFields, int size);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern void RaiseFileMonitoringEventlogEvent(string eventInfo, string path, string appVirtualPath, int hr);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int RaiseWmiEvent(ref WmiData pWmiData, bool IsInAspCompatMode);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern void ReportUnhandledException(string eventInfo);
        [DllImport("advapi32.dll")]
        internal static extern int RevertToSelf();
        [DllImport("webengine4.dll")]
        internal static extern void SessionNDCloseConnection(HandleRef socket);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern int SessionNDConnectToService(string server);
        [DllImport("webengine4.dll")]
        internal static extern void SessionNDFreeBody(HandleRef body);
        [DllImport("webengine4.dll", CharSet=CharSet.Ansi)]
        internal static extern int SessionNDMakeRequest(HandleRef socket, string server, int port, int networkTimeout, StateProtocolVerb verb, string uri, StateProtocolExclusive exclusive, int extraFlags, int timeout, int lockCookie, byte[] body, int cb, bool checkVersion, out SessionNDMakeRequestResults results);
        [DllImport("webengine4.dll")]
        internal static extern void SetClrThreadPoolLimits(int maxWorkerThreads, int maxIoThreads);
        [DllImport("webengine4.dll")]
        internal static extern void SetDoneWithSessionCalled(IntPtr pHttpCompletion);
        [DllImport("webengine4.dll")]
        internal static extern void SetMinRequestsExecutingToDetectDeadlock(int minRequestsExecutingToDetectDeadlock);
        [DllImport("advapi32.dll")]
        internal static extern int SetThreadToken(IntPtr threadref, IntPtr token);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int SizeofResource(IntPtr hModule, IntPtr hResInfo);
        [DllImport("aspnet_state.exe")]
        internal static extern void STWNDCloseConnection(IntPtr tracker);
        [DllImport("aspnet_state.exe")]
        internal static extern void STWNDDeleteStateItem(IntPtr stateItem);
        [DllImport("aspnet_state.exe")]
        internal static extern void STWNDEndOfRequest(IntPtr tracker);
        [DllImport("aspnet_state.exe", CharSet=CharSet.Ansi)]
        internal static extern void STWNDGetLocalAddress(IntPtr tracker, StringBuilder buf);
        [DllImport("aspnet_state.exe")]
        internal static extern int STWNDGetLocalPort(IntPtr tracker);
        [DllImport("aspnet_state.exe", CharSet=CharSet.Ansi)]
        internal static extern void STWNDGetRemoteAddress(IntPtr tracker, StringBuilder buf);
        [DllImport("aspnet_state.exe")]
        internal static extern int STWNDGetRemotePort(IntPtr tracker);
        [DllImport("aspnet_state.exe")]
        internal static extern bool STWNDIsClientConnected(IntPtr tracker);
        [DllImport("aspnet_state.exe", CharSet=CharSet.Unicode)]
        internal static extern void STWNDSendResponse(IntPtr tracker, StringBuilder status, int statusLength, StringBuilder headers, int headersLength, IntPtr unmanagedState);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern void TraceRaiseEventMgdHandler(int eventType, IntPtr pRequestContext, string data1, string data2, string data3, string data4);
        [DllImport("webengine4.dll", CharSet=CharSet.Unicode)]
        internal static extern void TraceRaiseEventWithEcb(int eventType, IntPtr ecb, string data1, string data2, string data3, string data4);
        [DllImport("webengine4.dll")]
        internal static extern int TransactManagedCallback(TransactedExecCallback callback, int mode);
        [DllImport("webengine4.dll")]
        internal static extern void UpdateLastActivityTimeForHealthMonitor();

        internal enum CallISAPIFunc
        {
            CreateTempDir = 3,
            GenerateToken = 5,
            GetAutogenKeys = 4,
            GetSiteServerComment = 1,
            RestrictIISFolders = 2
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct MEMORYSTATUSEX
        {
            internal int dwLength;
            internal int dwMemoryLoad;
            internal long ullTotalPhys;
            internal long ullAvailPhys;
            internal long ullTotalPageFile;
            internal long ullAvailPageFile;
            internal long ullTotalVirtual;
            internal long ullAvailVirtual;
            internal long ullAvailExtendedVirtual;
            internal void Init()
            {
                this.dwLength = Marshal.SizeOf(typeof(UnsafeNativeMethods.MEMORYSTATUSEX));
            }
        }

        internal enum SessionNDMakeRequestPhase
        {
            Initialization,
            Connecting,
            SendingRequest,
            ReadingResponse
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SessionNDMakeRequestResults
        {
            internal IntPtr socket;
            internal int httpStatus;
            internal int timeout;
            internal int contentLength;
            internal IntPtr content;
            internal int lockCookie;
            internal long lockDate;
            internal int lockAge;
            internal int stateServerMajVer;
            internal int actionFlags;
            internal int lastPhase;
        }

        internal enum StateProtocolExclusive
        {
            NONE,
            ACQUIRE,
            RELEASE
        }

        internal enum StateProtocolVerb
        {
            DELETE = 3,
            GET = 1,
            HEAD = 4,
            PUT = 2
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WIN32_BY_HANDLE_FILE_INFORMATION
        {
            internal int fileAttributes;
            internal uint ftCreationTimeLow;
            internal uint ftCreationTimeHigh;
            internal uint ftLastAccessTimeLow;
            internal uint ftLastAccessTimeHigh;
            internal uint ftLastWriteTimeLow;
            internal uint ftLastWriteTimeHigh;
            internal uint volumeSerialNumber;
            internal uint fileSizeHigh;
            internal uint fileSizeLow;
            internal uint numberOfLinks;
            internal uint fileIndexHigh;
            internal uint fileIndexLow;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WIN32_FILE_ATTRIBUTE_DATA
        {
            internal int fileAttributes;
            internal uint ftCreationTimeLow;
            internal uint ftCreationTimeHigh;
            internal uint ftLastAccessTimeLow;
            internal uint ftLastAccessTimeHigh;
            internal uint ftLastWriteTimeLow;
            internal uint ftLastWriteTimeHigh;
            internal uint fileSizeHigh;
            internal uint fileSizeLow;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct WIN32_FIND_DATA
        {
            internal uint dwFileAttributes;
            internal uint ftCreationTime_dwLowDateTime;
            internal uint ftCreationTime_dwHighDateTime;
            internal uint ftLastAccessTime_dwLowDateTime;
            internal uint ftLastAccessTime_dwHighDateTime;
            internal uint ftLastWriteTime_dwLowDateTime;
            internal uint ftLastWriteTime_dwHighDateTime;
            internal uint nFileSizeHigh;
            internal uint nFileSizeLow;
            internal uint dwReserved0;
            internal uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            internal string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=14)]
            internal string cAlternateFileName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct WmiData
        {
            internal int eventType;
            internal int eventCode;
            internal int eventDetailCode;
            internal string eventTime;
            internal string eventMessage;
            internal string eventId;
            internal string sequenceNumber;
            internal string occurrence;
            internal int processId;
            internal string processName;
            internal string accountName;
            internal string machineName;
            internal string appDomain;
            internal string trustLevel;
            internal string appVirtualPath;
            internal string appPath;
            internal string details;
            internal string requestUrl;
            internal string requestPath;
            internal string userHostAddress;
            internal string userName;
            internal bool userAuthenticated;
            internal string userAuthenticationType;
            internal string requestThreadAccountName;
            internal string processStartTime;
            internal int threadCount;
            internal string workingSet;
            internal string peakWorkingSet;
            internal string managedHeapSize;
            internal int appdomainCount;
            internal int requestsExecuting;
            internal int requestsQueued;
            internal int requestsRejected;
            internal int threadId;
            internal string threadAccountName;
            internal string stackTrace;
            internal bool isImpersonating;
            internal string exceptionType;
            internal string exceptionMessage;
            internal string nameToAuthenticate;
            internal string remoteAddress;
            internal string remotePort;
            internal string userAgent;
            internal string persistedState;
            internal string referer;
            internal string path;
        }
    }
}

