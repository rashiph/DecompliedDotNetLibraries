namespace System.Net
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal abstract class SafeFreeCredentials : SafeHandle
    {
        internal SSPIHandle _handle;

        protected SafeFreeCredentials() : base(IntPtr.Zero, true)
        {
            this._handle = new SSPIHandle();
        }

        public static int AcquireCredentialsHandle(SecurDll dll, string package, CredentialUse intent, ref AuthIdentity authdata, out SafeFreeCredentials outCredential)
        {
            long num2;
            int num = -1;
            switch (dll)
            {
                case SecurDll.SECURITY:
                    outCredential = new SafeFreeCredential_SECURITY();
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        goto Label_008D;
                    }
                    finally
                    {
                        num = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.AcquireCredentialsHandleW(null, package, (int) intent, null, ref authdata, null, null, ref outCredential._handle, out num2);
                    }
                    break;

                case SecurDll.SECUR32:
                    break;

                default:
                    goto Label_0068;
            }
            outCredential = new SafeFreeCredential_SECUR32();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                goto Label_008D;
            }
            finally
            {
                num = UnsafeNclNativeMethods.SafeNetHandles_SECUR32.AcquireCredentialsHandleA(null, package, (int) intent, null, ref authdata, null, null, ref outCredential._handle, out num2);
            }
        Label_0068:;
            throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "SecurDll" }), "Dll");
        Label_008D:
            if (num != 0)
            {
                outCredential.SetHandleAsInvalid();
            }
            return num;
        }

        public static unsafe int AcquireCredentialsHandle(SecurDll dll, string package, CredentialUse intent, ref SecureCredential authdata, out SafeFreeCredentials outCredential)
        {
            int num = -1;
            IntPtr certContextArray = authdata.certContextArray;
            try
            {
                long num2;
                IntPtr ptr2 = new IntPtr((void*) &certContextArray);
                if (certContextArray != IntPtr.Zero)
                {
                    authdata.certContextArray = ptr2;
                }
                switch (dll)
                {
                    case SecurDll.SECURITY:
                        outCredential = new SafeFreeCredential_SECURITY();
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try
                        {
                            goto Label_00C5;
                        }
                        finally
                        {
                            num = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.AcquireCredentialsHandleW(null, package, (int) intent, null, ref authdata, null, null, ref outCredential._handle, out num2);
                        }
                        break;

                    case SecurDll.SCHANNEL:
                        break;

                    default:
                        goto Label_0093;
                }
                outCredential = new SafeFreeCredential_SCHANNEL();
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    goto Label_00C5;
                }
                finally
                {
                    num = UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.AcquireCredentialsHandleA(null, package, (int) intent, null, ref authdata, null, null, ref outCredential._handle, out num2);
                }
            Label_0093:;
                throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "SecurDll" }), "Dll");
            }
            finally
            {
                authdata.certContextArray = certContextArray;
            }
        Label_00C5:
            if (num != 0)
            {
                outCredential.SetHandleAsInvalid();
            }
            return num;
        }

        public static int AcquireDefaultCredential(SecurDll dll, string package, CredentialUse intent, out SafeFreeCredentials outCredential)
        {
            long num2;
            int num = -1;
            switch (dll)
            {
                case SecurDll.SECURITY:
                    outCredential = new SafeFreeCredential_SECURITY();
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        goto Label_0091;
                    }
                    finally
                    {
                        num = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.AcquireCredentialsHandleW(null, package, (int) intent, null, IntPtr.Zero, null, null, ref outCredential._handle, out num2);
                    }
                    break;

                case SecurDll.SECUR32:
                    break;

                default:
                    goto Label_006C;
            }
            outCredential = new SafeFreeCredential_SECUR32();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                goto Label_0091;
            }
            finally
            {
                num = UnsafeNclNativeMethods.SafeNetHandles_SECUR32.AcquireCredentialsHandleA(null, package, (int) intent, null, IntPtr.Zero, null, null, ref outCredential._handle, out num2);
            }
        Label_006C:;
            throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "SecurDll" }), "Dll");
        Label_0091:
            if (num != 0)
            {
                outCredential.SetHandleAsInvalid();
            }
            return num;
        }

        public override bool IsInvalid
        {
            get
            {
                if (!base.IsClosed)
                {
                    return this._handle.IsZero;
                }
                return true;
            }
        }
    }
}

