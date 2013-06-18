namespace System.EnterpriseServices.Thunk
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class ServiceDomainThunk
    {
        internal static int modopt(IsLong) modopt(CallConvStdcall) *(IUnknown*, _GUID modopt(IsConst)* modopt(IsImplicitlyDereferenced), void**) CoCreateActivity;
        internal static int modopt(IsLong) modopt(CallConvStdcall) *(IUnknown*) CoEnterServiceDomain;
        internal static int modopt(IsLong) modopt(CallConvStdcall) *(IUnknown*) CoLeaveServiceDomain;

        static unsafe ServiceDomainThunk()
        {
            HINSTANCE__* hinstance__Ptr = LoadLibraryW(&??_C@_1BI@NMLGLHFF@?$AAc?$AAo?$AAm?$AAs?$AAv?$AAc?$AAs?$AA?4?$AAd?$AAl?$AAl?$AA?$AA@);
            if ((hinstance__Ptr == null) || (hinstance__Ptr == -1))
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
            CoEnterServiceDomain = GetProcAddress(hinstance__Ptr, &??_C@_0BF@EEGEFJCM@CoEnterServiceDomain?$AA@);
            CoLeaveServiceDomain = GetProcAddress(hinstance__Ptr, &??_C@_0BF@JEIDNIFH@CoLeaveServiceDomain?$AA@);
            CoCreateActivity = GetProcAddress(hinstance__Ptr, &??_C@_0BB@LLBGKOGP@CoCreateActivity?$AA@);
        }

        private ServiceDomainThunk()
        {
        }

        public static unsafe void EnterServiceDomain(ServiceConfigThunk psct)
        {
            IUnknown* serviceConfigUnknown = psct.ServiceConfigUnknown;
            **(((int*) serviceConfigUnknown))[8](serviceConfigUnknown);
            Marshal.ThrowExceptionForHR(*CoEnterServiceDomain(serviceConfigUnknown));
        }

        public static unsafe int LeaveServiceDomain()
        {
            int modopt(IsLong) num = 0;
            TransactionStatus* statusPtr = System.EnterpriseServices.Thunk.TransactionStatus.CreateInstance();
            if (statusPtr == null)
            {
                throw new OutOfMemoryException();
            }
            num = *CoLeaveServiceDomain(statusPtr);
            if (num >= 0)
            {
                **(((int*) statusPtr))[0x10](statusPtr, &num);
            }
            **(((int*) statusPtr))[8](statusPtr);
            return num;
        }
    }
}

