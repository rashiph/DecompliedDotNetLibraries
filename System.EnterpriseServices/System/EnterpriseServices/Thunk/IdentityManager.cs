namespace System.EnterpriseServices.Thunk
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class IdentityManager
    {
        private IdentityManager()
        {
        }

        public static unsafe string CreateIdentityUri(IntPtr pUnk)
        {
            ulong num2;
            ulong num3;
            int modopt(IsLong) errorCode = System.EnterpriseServices.Thunk.ReadIdentity((IUnknown*) pUnk.ToInt32(), &num3, &num2);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            ulong num5 = num2;
            ulong num4 = num3;
            return ("servicedcomponent-local-identity://" + num4.ToString(CultureInfo.InvariantCulture) + ":" + num5.ToString(CultureInfo.InvariantCulture));
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public static unsafe bool IsInProcess(IntPtr pUnk)
        {
            byte num2;
            int num3 = 1;
            IRpcOptions* optionsPtr = null;
            IUnknown* unknownPtr = (IUnknown*) pUnk.ToInt32();
            int modopt(IsLong) errorCode = **(*((int*) unknownPtr))(unknownPtr, &_GUID_00000144_0000_0000_c000_000000000046, &optionsPtr);
            if (errorCode >= 0)
            {
                uint modopt(IsLong) num4 = 0;
                errorCode = **(((int*) optionsPtr))[0x10](optionsPtr, unknownPtr, 2, &num4);
                **(((int*) optionsPtr))[8](optionsPtr);
                if (errorCode < 0)
                {
                    goto Label_005B;
                }
                if (num4 != null)
                {
                    num3 = 0;
                }
            }
            else if (errorCode == -2147467262)
            {
                goto Label_0065;
            }
            if (errorCode >= 0)
            {
                goto Label_0061;
            }
        Label_005B:
            Marshal.ThrowExceptionForHR(errorCode);
        Label_0061:
            if (num3 == 0)
            {
                num2 = 0;
                goto Label_006B;
            }
        Label_0065:
            num2 = 1;
        Label_006B:
            return (bool) num2;
        }
    }
}

