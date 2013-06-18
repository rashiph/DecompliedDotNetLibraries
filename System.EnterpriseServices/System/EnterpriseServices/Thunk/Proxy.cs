namespace System.EnterpriseServices.Thunk
{
    using <CppImplementationDetails>;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Threading;

    internal class Proxy
    {
        private static object _classSyncRoot;
        private static bool _fInit;
        private unsafe static IGlobalInterfaceTable* _pGIT;
        private static Hashtable _regCache;
        private static Mutex _regmutex;
        private static Assembly _thisAssembly;
        public static int INFO_APPDOMAINID = 2;
        public static int INFO_PROCESSID = 1;
        public static int INFO_URI = 4;

        private Proxy()
        {
        }

        public static int CallFunction(IntPtr pfn, IntPtr data)
        {
            return *pfn.ToInt32()(data.ToInt32());
        }

        [return: MarshalAs(UnmanagedType.U1)]
        private static unsafe bool CheckRegistered(Guid id, Assembly assembly, [MarshalAs(UnmanagedType.U1)] bool checkCache, [MarshalAs(UnmanagedType.U1)] bool cacheOnly)
        {
            if (checkCache && (_regCache[assembly] != null))
            {
                return true;
            }
            if (cacheOnly)
            {
                return false;
            }
            bool flag = false;
            string name = @"CLSID\{" + id.ToString() + @"}\InprocServer32";
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(name, false);
            if (key != null)
            {
                _regCache[assembly] = bool.TrueString;
            }
            else if (IsWin64(&flag))
            {
                HKEY__* hkey__Ptr;
                IntPtr hglobal = Marshal.StringToHGlobalUni(name);
                char* chPtr = (char*) hglobal.ToPointer();
                int num = flag ? 0x100 : 0x200;
                Marshal.FreeHGlobal(hglobal);
                if (RegOpenKeyExW(-2147483648, (char modopt(IsConst)*) chPtr, 0, (uint modopt(IsLong)) (num | 0x20019), &hkey__Ptr) != 0)
                {
                    return false;
                }
                RegCloseKey(hkey__Ptr);
                return true;
            }
            return ((key != null) ? ((bool) ((byte) 1)) : ((bool) ((byte) 0)));
        }

        public static unsafe IntPtr CoCreateObject(Type serverType, [MarshalAs(UnmanagedType.U1)] bool bQuerySCInfo, ref bool bIsAnotherProcess, ref string uri)
        {
            Init();
            IUnknown* unknownPtr2 = null;
            bool checkCache = true;
            Guid id = Marshal.GenerateGuidForType(serverType);
            do
            {
                IUnknown* unknownPtr = null;
                IServicedComponentInfo* infoPtr = null;
                tagSAFEARRAY* gsafearrayPtr = null;
                try
                {
                    $ArrayType$$$BY00UtagMULTI_QI@@ gmulti_qi@@;
                    _GUID _guid;
                    LazyRegister(id, serverType, checkCache);
                    memcpy(&_guid, ((int) &id), 0x10);
                    *((int*) &gmulti_qi@@) = 0;
                    meminit((&gmulti_qi@@ + 4), 0, 8);
                    *((int*) &gmulti_qi@@) = &IID_IUnknown;
                    int modopt(IsLong) errorCode = CoCreateInstanceEx((_GUID modopt(IsConst)* modopt(IsImplicitlyDereferenced)) &_guid, null, 0x17, null, 1, (tagMULTI_QI*) &gmulti_qi@@);
                    if (errorCode >= 0)
                    {
                        if (*(((int*) (&gmulti_qi@@ + 8))) >= 0)
                        {
                            unknownPtr = *((IUnknown**) (&gmulti_qi@@ + 4));
                        }
                        else
                        {
                            int modopt(IsLong) num3 = *((int*) (&gmulti_qi@@ + 8));
                            Marshal.ThrowExceptionForHR(*((int*) (&gmulti_qi@@ + 8)));
                        }
                    }
                    else
                    {
                        if ((errorCode == -2147221164) && checkCache)
                        {
                            checkCache = false;
                        }
                        else
                        {
                            Marshal.ThrowExceptionForHR(errorCode);
                        }
                        goto Label_00C2;
                    }
                    if (bQuerySCInfo)
                    {
                        IntPtr pUnk = new IntPtr((int) unknownPtr);
                        byte num2 = (byte) !IdentityManager.IsInProcess(pUnk);
                        bIsAnotherProcess = (bool) num2;
                        if (num2 != 0)
                        {
                            IntPtr ptr2 = new IntPtr((int) unknownPtr);
                            uri = IdentityManager.CreateIdentityUri(ptr2);
                        }
                    }
                    else
                    {
                        bIsAnotherProcess = true;
                    }
                Label_00C2:
                    unknownPtr2 = unknownPtr;
                    unknownPtr = null;
                }
                finally
                {
                    if (unknownPtr != null)
                    {
                        **(((int*) unknownPtr))[8](unknownPtr);
                    }
                    if (infoPtr != null)
                    {
                        **(((int*) infoPtr))[8](infoPtr);
                    }
                    if (gsafearrayPtr != null)
                    {
                        SafeArrayDestroy(gsafearrayPtr);
                    }
                }
            }
            while (unknownPtr2 == null);
            return new IntPtr((int) unknownPtr2);
        }

        [HandleProcessCorruptedStateExceptions]
        public static unsafe Tracker FindTracker(IntPtr ctx)
        {
            _GUID modopt(IsConst) _guid;
            *((int*) &_guid) = -324292941;
            *((short*) (&_guid + 4)) = 0x7f19;
            *((short*) (&_guid + 6)) = 0x11d2;
            *((sbyte*) (&_guid + 8)) = 0x97;
            *((sbyte*) (&_guid + 9)) = 0x8e;
            *((sbyte*) (&_guid + 10)) = 0;
            *((sbyte*) (&_guid + 11)) = 0;
            *((sbyte*) (&_guid + 12)) = 0xf8;
            *((sbyte*) (&_guid + 13)) = 0x75;
            *((sbyte*) (&_guid + 14)) = 0x7e;
            *((sbyte*) (&_guid + 15)) = 0x2a;
            IUnknown* unknownPtr = null;
            ISendMethodEvents* pTracker = null;
            IObjContext* contextPtr = null;
            uint modopt(IsLong) num3 = 0;
            try
            {
                int num2 = ctx.ToInt32();
                if (**(num2[0])(num2, &System.EnterpriseServices.Thunk.?A0x9f41c966.IID_IObjContext, &contextPtr) >= 0)
                {
                    if ((**(((int*) contextPtr))[20](contextPtr, &_guid, &num3, &unknownPtr) >= 0) && (unknownPtr != null))
                    {
                        if (**(*(((int*) unknownPtr)))(unknownPtr, &_GUID_2732fd59_b2b4_4d44_878c_8b8f09626008, &pTracker) < 0)
                        {
                            pTracker = null;
                            return null;
                        }
                        return new Tracker(pTracker);
                    }
                    unknownPtr = null;
                }
                return null;
            }
            finally
            {
                if (contextPtr != null)
                {
                    **(((int*) contextPtr))[8](contextPtr);
                }
                if (unknownPtr != null)
                {
                    **(((int*) unknownPtr))[8](unknownPtr);
                }
                if (pTracker != null)
                {
                    **(((int*) pTracker))[8](pTracker);
                }
            }
            return null;
        }

        public static IntPtr GetContextCheck()
        {
            Init();
            return new IntPtr((int) GetContextCheck());
        }

        public static unsafe IntPtr GetCurrentContext()
        {
            IUnknown* unknownPtr;
            Init();
            int modopt(IsLong) context = GetContext(&IID_IUnknown, (void**) &unknownPtr);
            if (context < 0)
            {
                Marshal.ThrowExceptionForHR(context);
            }
            return new IntPtr((int) unknownPtr);
        }

        public static IntPtr GetCurrentContextToken()
        {
            Init();
            return new IntPtr((int) GetContextToken());
        }

        public static unsafe int GetManagedExts()
        {
            if (?A0x9f41c966.?dwExts@?1??GetManagedExts@Proxy@Thunk@EnterpriseServices@System@@SMHXZ@4KA == uint.MaxValue)
            {
                uint modopt(IsLong) num = 0;
                HINSTANCE__* hinstance__Ptr = LoadLibraryW(&?A0x9f41c966.unnamed-global-8);
                if ((hinstance__Ptr != null) && (hinstance__Ptr != -1))
                {
                    int modopt(IsLong) modopt(CallConvStdcall) *(uint modopt(IsLong)*) procAddress = GetProcAddress(hinstance__Ptr, &?A0x9f41c966.unnamed-global-9);
                    if ((procAddress != null) && (*procAddress(&num) < 0))
                    {
                        num = 0;
                    }
                }
                ?A0x9f41c966.?dwExts@?1??GetManagedExts@Proxy@Thunk@EnterpriseServices@System@@SMHXZ@4KA = num;
            }
            return ?A0x9f41c966.?dwExts@?1??GetManagedExts@Proxy@Thunk@EnterpriseServices@System@@SMHXZ@4KA;
        }

        public static unsafe int GetMarshalSize(object o)
        {
            Init();
            IUnknown* unknownPtr = null;
            uint modopt(IsLong) maxValue = 0;
            try
            {
                unknownPtr = (IUnknown*) Marshal.GetIUnknownForObject(o).ToInt32();
                if (CoGetMarshalSizeMax(&maxValue, &IID_IUnknown, unknownPtr, 2, null, 0) >= 0)
                {
                    return (maxValue + 4);
                }
                maxValue = uint.MaxValue;
            }
            finally
            {
                if (unknownPtr != null)
                {
                    **(((int*) unknownPtr))[8](unknownPtr);
                }
            }
            return maxValue;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        private static unsafe bool GetNativeSystemInfoInternal(_SYSTEM_INFO* _system_infoPtr1)
        {
            void modopt(CallConvStdcall) *(_SYSTEM_INFO*) procAddress = GetProcAddress(GetModuleHandleW(&?A0x9f41c966.unnamed-global-4), &?A0x9f41c966.unnamed-global-3);
            if (procAddress == null)
            {
                return false;
            }
            *procAddress(_system_infoPtr1);
            return true;
        }

        public static unsafe IntPtr GetObject(int cookie)
        {
            Init();
            IUnknown* unknownPtr = null;
            int num2 = *(((int*) _pGIT)) + 20;
            int modopt(IsLong) errorCode = *num2[0](_pGIT, cookie, &IID_IUnknown, &unknownPtr);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            return new IntPtr((int) unknownPtr);
        }

        public static unsafe IntPtr GetStandardMarshal(IntPtr pUnk)
        {
            IMarshal* marshalPtr;
            int modopt(IsLong) errorCode = CoGetStandardMarshal(&IID_IUnknown, (IUnknown*) pUnk.ToInt32(), 2, null, 0, &marshalPtr);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            return new IntPtr((int) marshalPtr);
        }

        public static unsafe void Init()
        {
            object obj2 = null;
            int num3 = (int) stackalloc byte[__CxxQueryExceptionSize()];
            if (Thread.CurrentThread.ApartmentState == ApartmentState.Unknown)
            {
                Thread.CurrentThread.ApartmentState = ApartmentState.MTA;
            }
            if (!_fInit)
            {
                try
                {
                    IntPtr zero = IntPtr.Zero;
                    if (_classSyncRoot == null)
                    {
                        obj2 = new object();
                        Interlocked.CompareExchange(ref _classSyncRoot, obj2, null);
                    }
                    lock (_classSyncRoot)
                    {
                        try
                        {
                            zero = Security.SuspendImpersonation();
                            if (!_fInit)
                            {
                                _regCache = new Hashtable();
                                IGlobalInterfaceTable* tablePtr = null;
                                int modopt(IsLong) errorCode = CoCreateInstance(&CLSID_StdGlobalInterfaceTable, null, 1, &IID_IGlobalInterfaceTable, (void**) &tablePtr);
                                _pGIT = tablePtr;
                                if (errorCode < 0)
                                {
                                    Marshal.ThrowExceptionForHR(errorCode);
                                }
                                _thisAssembly = Assembly.GetExecutingAssembly();
                                _regmutex = new Mutex(false, @"Local\" + RemotingConfiguration.ProcessId);
                                Thread.MemoryBarrier();
                                _fInit = true;
                            }
                        }
                        finally
                        {
                            Security.ResumeImpersonation(zero);
                        }
                    }
                }
                catch when (?)
                {
                    uint num = 0;
                    __CxxRegisterExceptionObject((void*) Marshal.GetExceptionPointers(), (void*) num3);
                    try
                    {
                        try
                        {
                            _CxxThrowException(null, null);
                        }
                        catch when (?)
                        {
                        }
                        return;
                        if (num != 0)
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        __CxxUnregisterExceptionObject((void*) num3, (int) num);
                    }
                }
            }
        }

        [return: MarshalAs(UnmanagedType.U1)]
        private static unsafe bool IsWin64(bool* modopt(IsImplicitlyDereferenced) local1)
        {
            if (?A0x9f41c966.?fInit@?1??IsWin64@Proxy@Thunk@EnterpriseServices@System@@CM_NAA_N@Z@4HC == 0)
            {
                int num;
                local1[0] = 0;
                void modopt(CallConvStdcall) *(_SYSTEM_INFO*) procAddress = GetProcAddress(GetModuleHandleW(&?A0x9f41c966.unnamed-global-4), &?A0x9f41c966.unnamed-global-3);
                if (procAddress != null)
                {
                    _SYSTEM_INFO _system_info;
                    *procAddress(&_system_info);
                    if ((*(((ushort*) &_system_info)) != 6) && (*(((ushort*) &_system_info)) != 9))
                    {
                        num = 0;
                    }
                    else
                    {
                        int num2 = 0;
                        int modopt(CallConvStdcall) *(void*, int*) local = GetProcAddress(GetModuleHandleW(&?A0x9f41c966.unnamed-global-2), &?A0x9f41c966.unnamed-global-1);
                        if (local == null)
                        {
                            num2 = 0;
                        }
                        else if (*local(GetCurrentProcess(), &num2) == null)
                        {
                            num2 = 0;
                        }
                        else if (num2 == 1)
                        {
                            local1[0] = 1;
                        }
                        num = 1;
                    }
                }
                else
                {
                    num = 0;
                }
                ?A0x9f41c966.?fWin64@?1??IsWin64@Proxy@Thunk@EnterpriseServices@System@@CM_NAA_N@Z@4HC = num;
                ?A0x9f41c966.?fIsWow@?1??IsWin64@Proxy@Thunk@EnterpriseServices@System@@CM_NAA_N@Z@4HC = (local1[0] != 0) ? 1 : 0;
                ?A0x9f41c966.?fInit@?1??IsWin64@Proxy@Thunk@EnterpriseServices@System@@CM_NAA_N@Z@4HC = 1;
                byte num5 = (num != 0) ? ((byte) 1) : ((byte) 0);
                return (bool) num5;
            }
            byte num4 = (?A0x9f41c966.?fIsWow@?1??IsWin64@Proxy@Thunk@EnterpriseServices@System@@CM_NAA_N@Z@4HC != 0) ? ((byte) 1) : ((byte) 0);
            local1[0] = (bool* modopt(IsImplicitlyDereferenced)) num4;
            byte num3 = (?A0x9f41c966.?fWin64@?1??IsWin64@Proxy@Thunk@EnterpriseServices@System@@CM_NAA_N@Z@4HC != 0) ? ((byte) 1) : ((byte) 0);
            return (bool) num3;
        }

        private static unsafe void IsWow64ProcessInternal(int* numPtr1)
        {
            int modopt(CallConvStdcall) *(void*, int*) procAddress = GetProcAddress(GetModuleHandleW(&?A0x9f41c966.unnamed-global-2), &?A0x9f41c966.unnamed-global-1);
            if (procAddress == null)
            {
                numPtr1[0] = 0;
            }
            else if (*procAddress(GetCurrentProcess(), numPtr1) == null)
            {
                numPtr1[0] = 0;
            }
        }

        private static void LazyRegister(Guid id, Type serverType, [MarshalAs(UnmanagedType.U1)] bool checkCache)
        {
            if (serverType.Assembly != _thisAssembly)
            {
                Assembly assembly = serverType.Assembly;
                Guid guid = id;
                if (!checkCache || (_regCache[assembly] == null))
                {
                    _regmutex.WaitOne();
                    try
                    {
                        if (!CheckRegistered(id, serverType.Assembly, checkCache, false))
                        {
                            RegisterAssembly(serverType.Assembly);
                        }
                    }
                    finally
                    {
                        _regmutex.ReleaseMutex();
                    }
                }
            }
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public static unsafe bool MarshalObject(object o, byte[] b, int cb)
        {
            Init();
            IUnknown* unknownPtr = null;
            fixed (byte* numRef = b)
            {
                byte* numPtr = numRef;
                try
                {
                    unknownPtr = (IUnknown*) Marshal.GetIUnknownForObject(o).ToInt32();
                    int modopt(IsLong) errorCode = MarshalInterface(numPtr, cb, unknownPtr, 2, 0);
                    if (errorCode < 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                finally
                {
                    if (unknownPtr != null)
                    {
                        **(((int*) unknownPtr))[8](unknownPtr);
                    }
                }
                return true;
            }
        }

        public static unsafe void PoolMark(IntPtr pPooledObject)
        {
            IManagedPooledObj* objPtr = (IManagedPooledObj*) pPooledObject.ToInt32();
            **(((int*) objPtr))[12](objPtr, 1);
        }

        public static unsafe void PoolUnmark(IntPtr pPooledObject)
        {
            IManagedPooledObj* objPtr = (IManagedPooledObj*) pPooledObject.ToInt32();
            **(((int*) objPtr))[12](objPtr, 0);
        }

        private static void RegisterAssembly(Assembly assembly)
        {
            try
            {
                ((IThunkInstallation) Activator.CreateInstance(Type.GetType("System.EnterpriseServices.RegistrationHelper"))).DefaultInstall(assembly.Location);
            }
            finally
            {
                _regCache[assembly] = bool.TrueString;
            }
        }

        public static int RegisterProxyStub()
        {
            return DllRegisterServer();
        }

        public static unsafe void ReleaseMarshaledObject(byte[] b)
        {
            Init();
            fixed (byte* numRef = b)
            {
                byte* numPtr = numRef;
                try
                {
                    int modopt(IsLong) errorCode = ReleaseMarshaledInterface(numPtr, b.Length);
                    if (errorCode < 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                finally
                {
                }
                return;
            }
        }

        public static unsafe void RevokeObject(int cookie)
        {
            Init();
            int num2 = *(((int*) _pGIT)) + 0x10;
            int modopt(IsLong) errorCode = *num2[0](_pGIT, cookie);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }

        public static unsafe void SendCreationEvents(IntPtr ctx, IntPtr stub, [MarshalAs(UnmanagedType.U1)] bool fDist)
        {
            IUnknown* unknownPtr = (IUnknown*) ctx.ToInt32();
            IObjContext* contextPtr = null;
            IManagedObjectInfo* infoPtr = (IManagedObjectInfo*) stub.ToInt32();
            IEnumContextProps* propsPtr = null;
            if (**(*(((int*) unknownPtr)))(unknownPtr, &System.EnterpriseServices.Thunk.?A0x9f41c966.IID_IObjContext, &contextPtr) >= 0)
            {
                try
                {
                    if (**(((int*) contextPtr))[0x18](contextPtr, &propsPtr) >= 0)
                    {
                        uint modopt(IsLong) num4 = 0;
                        int modopt(IsLong) errorCode = **(((int*) propsPtr))[0x1c](propsPtr, &num4);
                        if (errorCode < 0)
                        {
                            Marshal.ThrowExceptionForHR(errorCode);
                        }
                        for (uint modopt(IsLong) i = 0; i < num4; i++)
                        {
                            tagContextProperty property;
                            uint modopt(IsLong) num3 = 0;
                            errorCode = **(((int*) propsPtr))[12](propsPtr, 1, &property, &num3);
                            if (errorCode < 0)
                            {
                                Marshal.ThrowExceptionForHR(errorCode);
                            }
                            if (num3 != 1)
                            {
                                return;
                            }
                            IManagedActivationEvents* eventsPtr = null;
                            if (**(*(*(((int*) (&property + 20)))))(*((int*) (&property + 20)), &IID_IManagedActivationEvents, &eventsPtr) >= 0)
                            {
                                **(((int*) eventsPtr))[12](eventsPtr, infoPtr, fDist);
                                **(((int*) eventsPtr))[8](eventsPtr);
                            }
                            **(*(((int*) (&property + 20))))[8](*((int*) (&property + 20)));
                        }
                    }
                }
                finally
                {
                    if (contextPtr != null)
                    {
                        **(((int*) contextPtr))[8](contextPtr);
                    }
                    if (propsPtr != null)
                    {
                        **(((int*) propsPtr))[8](propsPtr);
                    }
                }
            }
        }

        [HandleProcessCorruptedStateExceptions]
        public static unsafe void SendDestructionEvents(IntPtr ctx, IntPtr stub, [MarshalAs(UnmanagedType.U1)] bool disposing)
        {
            tagComCallData data;
            DestructData data2;
            *((int*) &data2) = ctx.ToInt32();
            *((int*) (&data2 + 4)) = stub.ToInt32();
            *((int*) &data) = 0;
            *((int*) (&data + 4)) = 0;
            *((int*) (&data + 8)) = &data2;
            IContextCallback* callbackPtr = null;
            int modopt(IsLong) errorCode = 0;
            try
            {
                _GUID modopt(IsConst)* _guidPtr;
                _GUID _guid;
                errorCode = **(*(*((int*) &data2)))(*((int*) &data2), &IID_IContextCallback, &callbackPtr);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                if (disposing)
                {
                    _guidPtr = &IID_IUnknown;
                }
                else
                {
                    _guidPtr = &IID_IEnterActivityWithNoLock;
                }
                _GUID modopt(IsConst)* _guidPtr2 = _guidPtr;
                memcpy(&_guid, _guidPtr, 0x10);
                errorCode = **(((int*) callbackPtr))[12](callbackPtr, __unep@?SendDestructionEventsCallback@Thunk@EnterpriseServices@System@@$$FYGJPAUtagComCallData@123@@Z, &data, &_guid, 2, 0);
            }
            finally
            {
                if (callbackPtr != null)
                {
                    **(((int*) callbackPtr))[8](callbackPtr);
                }
            }
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }

        public static unsafe int StoreObject(IntPtr ptr)
        {
            uint modopt(IsLong) num2;
            Init();
            IUnknown* unknownPtr = (IUnknown*) ptr.ToInt32();
            int num3 = *(((int*) _pGIT)) + 12;
            int modopt(IsLong) errorCode = *num3[0](_pGIT, unknownPtr, &IID_IUnknown, &num2);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            return num2;
        }

        public static unsafe IntPtr UnmarshalObject(byte[] b)
        {
            Init();
            IUnknown* unknownPtr = null;
            int length = b.Length;
            fixed (byte* numRef = b)
            {
                byte* numPtr = numRef;
                try
                {
                    int modopt(IsLong) errorCode = UnmarshalInterface(numPtr, length, (void**) &unknownPtr);
                    if (errorCode < 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                finally
                {
                }
                return new IntPtr((int) unknownPtr);
            }
        }
    }
}

