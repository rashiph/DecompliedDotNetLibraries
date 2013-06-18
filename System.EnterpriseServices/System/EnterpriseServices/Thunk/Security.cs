namespace System.EnterpriseServices.Thunk
{
    using <CppImplementationDetails>;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class Security
    {
        private static uint modopt(IsLong) _cPackages;
        private static int _fInit = 0;
        private unsafe static _SecPkgInfoW* _pPackageInfo;
        private static int modopt(CallConvStdcall) *(void*, uint modopt(IsLong), int, void**) OpenThreadToken = 0;
        private static int modopt(CallConvStdcall) *(void**, void*) SetThreadToken = 0;

        static Security()
        {
            _fInit = 0;
        }

        private Security()
        {
        }

        public static unsafe string GetEveryoneAccountName()
        {
            int num3;
            $ArrayType$$$BY0BAE@_W e$$$bybae@_w;
            $ArrayType$$$BY0BAE@_W e$$$bybae@_w2;
            _SID1 _sid;
            *((sbyte*) &_sid) = 1;
            *((sbyte*) (&_sid + 1)) = 1;
            *((sbyte*) (&_sid + 2)) = 0;
            *((sbyte*) (&_sid + 3)) = 0;
            *((sbyte*) (&_sid + 4)) = 0;
            *((sbyte*) (&_sid + 5)) = 0;
            *((sbyte*) (&_sid + 6)) = 0;
            *((sbyte*) (&_sid + 7)) = 1;
            *((int*) (&_sid + 8)) = 0;
            uint modopt(IsLong) num5 = 260;
            uint modopt(IsLong) num4 = 260;
            if (LookupAccountSidW(null, (void*) &_sid, (char*) &e$$$bybae@_w2, &num4, (char*) &e$$$bybae@_w, &num5, &num3) == 0)
            {
                int num;
                uint modopt(IsLong) lastError = GetLastError();
                if (lastError <= 0)
                {
                    num = lastError;
                }
                else
                {
                    num = (((int) lastError) & 0xffff) | -2147024896;
                }
                if (num < 0)
                {
                    Marshal.ThrowExceptionForHR(num);
                }
            }
            IntPtr ptr = new IntPtr((int) &e$$$bybae@_w2);
            return Marshal.PtrToStringUni(ptr);
        }

        private static unsafe int modopt(IsLong) Init()
        {
            if (_fInit == 0)
            {
                lock (typeof(Security))
                {
                    if (_fInit == 0)
                    {
                        _cPackages = 0;
                        HINSTANCE__* hinstance__Ptr = LoadLibraryW(&?A0xb23e9562.unnamed-global-0);
                        if ((hinstance__Ptr != null) && (hinstance__Ptr != -1))
                        {
                            OpenThreadToken = GetProcAddress(hinstance__Ptr, &?A0xb23e9562.unnamed-global-1);
                            SetThreadToken = GetProcAddress(hinstance__Ptr, &?A0xb23e9562.unnamed-global-2);
                        }
                        _fInit = 1;
                    }
                }
            }
            return 0;
        }

        public static unsafe void ResumeImpersonation(IntPtr hToken)
        {
            if ((OpenThreadToken != null) && (SetThreadToken != null))
            {
                IntPtr ptr = new IntPtr(0);
                if (hToken != ptr)
                {
                    *SetThreadToken(0, hToken.ToInt32());
                    CloseHandle((void*) hToken.ToInt32());
                }
            }
        }

        public static unsafe IntPtr SuspendImpersonation()
        {
            void* voidPtr = null;
            int modopt(IsLong) errorCode = Init();
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            if (((OpenThreadToken != null) && (SetThreadToken != null)) && (*OpenThreadToken(GetCurrentThread(), 4, 1, &voidPtr) != null))
            {
                *SetThreadToken(0, 0);
                return new IntPtr((int) voidPtr);
            }
            return IntPtr.Zero;
        }
    }
}

