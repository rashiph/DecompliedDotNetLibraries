namespace System.IdentityModel
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class SafeFreeCredentials : SafeHandle
    {
        internal SSPIHandle _handle;
        private const string SECURITY = "security.Dll";

        protected SafeFreeCredentials() : base(IntPtr.Zero, true)
        {
            this._handle = new SSPIHandle();
        }

        public static int AcquireCredentialsHandle(string package, CredentialUse intent, ref AuthIdentityEx authdata, out SafeFreeCredentials outCredential)
        {
            int num = -1;
            outCredential = new SafeFreeCredentials();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                long num2;
                num = AcquireCredentialsHandleW(null, package, (int) intent, null, ref authdata, null, null, ref outCredential._handle, out num2);
                if (num != 0)
                {
                    outCredential.SetHandleAsInvalid();
                }
            }
            return num;
        }

        public static unsafe int AcquireCredentialsHandle(string package, CredentialUse intent, ref SecureCredential authdata, out SafeFreeCredentials outCredential)
        {
            int num = -1;
            IntPtr certContextArray = authdata.certContextArray;
            try
            {
                IntPtr ptr2 = new IntPtr((void*) &certContextArray);
                if (certContextArray != IntPtr.Zero)
                {
                    authdata.certContextArray = ptr2;
                }
                outCredential = new SafeFreeCredentials();
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    long num2;
                    num = AcquireCredentialsHandleW(null, package, (int) intent, null, ref authdata, null, null, ref outCredential._handle, out num2);
                    if (num != 0)
                    {
                        outCredential.SetHandleAsInvalid();
                    }
                }
            }
            finally
            {
                authdata.certContextArray = certContextArray;
            }
            return num;
        }

        public static int AcquireCredentialsHandle(string package, CredentialUse intent, ref IntPtr ppAuthIdentity, out SafeFreeCredentials outCredential)
        {
            int num = -1;
            outCredential = new SafeFreeCredentials();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                long num2;
                num = AcquireCredentialsHandleW(null, package, (int) intent, null, ppAuthIdentity, null, null, ref outCredential._handle, out num2);
                if (num != 0)
                {
                    outCredential.SetHandleAsInvalid();
                }
            }
            return num;
        }

        [DllImport("security.Dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern unsafe int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref AuthIdentityEx authdata, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);
        [DllImport("security.Dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern unsafe int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] IntPtr zero, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);
        [DllImport("security.Dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern unsafe int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref SecureCredential authData, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);
        public static int AcquireDefaultCredential(string package, CredentialUse intent, ref AuthIdentityEx authIdentity, out SafeFreeCredentials outCredential)
        {
            int num = -1;
            outCredential = new SafeFreeCredentials();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                long num2;
                num = AcquireCredentialsHandleW(null, package, (int) intent, null, ref authIdentity, null, null, ref outCredential._handle, out num2);
                if (num != 0)
                {
                    outCredential.SetHandleAsInvalid();
                }
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("security.Dll", SetLastError=true, ExactSpelling=true)]
        internal static extern int FreeCredentialsHandle(ref SSPIHandle handlePtr);
        protected override bool ReleaseHandle()
        {
            return (FreeCredentialsHandle(ref this._handle) == 0);
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

