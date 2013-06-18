namespace System.EnterpriseServices.Thunk
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class SWCThunk
    {
        private SWCThunk()
        {
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public static unsafe bool IsSWCSupported()
        {
            IUnknown* unknownPtr = null;
            IServiceTransactionConfig* configPtr = null;
            int modopt(IsLong) errorCode = CoCreateInstance(&CLSID_CServiceConfig, null, 1, &IID_IUnknown, (void**) &unknownPtr);
            if (errorCode == -2147221164)
            {
                return false;
            }
            if (errorCode >= 0)
            {
                errorCode = **(*((int*) unknownPtr))(unknownPtr, &IID_IServiceTransactionConfig, &configPtr);
                if (errorCode == -2147467262)
                {
                    **(((int*) unknownPtr))[8](unknownPtr);
                    return false;
                }
            }
            if (configPtr != null)
            {
                **(((int*) configPtr))[8](configPtr);
            }
            if (unknownPtr != null)
            {
                **(((int*) unknownPtr))[8](unknownPtr);
            }
            Marshal.ThrowExceptionForHR(errorCode);
            return true;
        }
    }
}

