namespace System.IdentityModel
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel.Diagnostics;

    internal sealed class SafeFreeContextBuffer : SafeHandleZeroOrMinusOneIsInvalid
    {
        private const string SECURITY = "security.dll";

        private SafeFreeContextBuffer() : base(true)
        {
        }

        internal static SafeFreeContextBuffer CreateEmptyHandle()
        {
            return new SafeFreeContextBuffer();
        }

        internal static int EnumeratePackages(out int pkgnum, out SafeFreeContextBuffer pkgArray)
        {
            int num = -1;
            num = EnumerateSecurityPackagesW(out pkgnum, out pkgArray);
            if (num != 0)
            {
                Utility.CloseInvalidOutSafeHandle(pkgArray);
                pkgArray = null;
            }
            return num;
        }

        [DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern int EnumerateSecurityPackagesW(out int pkgnum, out SafeFreeContextBuffer handle);
        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
        private static extern int FreeContextBuffer([In] IntPtr contextBuffer);
        public static unsafe int QueryContextAttributes(SafeDeleteContext phContext, ContextAttribute contextAttribute, byte* buffer, SafeHandle refHandle)
        {
            int num = -2146893055;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                phContext.DangerousAddRef(ref success);
            }
            catch (Exception exception)
            {
                if (success)
                {
                    phContext.DangerousRelease();
                    success = false;
                }
                if (!(exception is ObjectDisposedException))
                {
                    throw;
                }
            }
            finally
            {
                if (success)
                {
                    num = QueryContextAttributesW(ref phContext._handle, contextAttribute, (void*) buffer);
                    phContext.DangerousRelease();
                }
                if ((num == 0) && (refHandle != null))
                {
                    if (refHandle is SafeFreeContextBuffer)
                    {
                        if (contextAttribute == ContextAttribute.SessionKey)
                        {
                            IntPtr ptr = Marshal.ReadIntPtr(new IntPtr((void*) buffer), SecPkgContext_SessionKey.SessionkeyOffset);
                            ((SafeFreeContextBuffer) refHandle).Set(ptr);
                        }
                        else
                        {
                            ((SafeFreeContextBuffer) refHandle).Set(*((IntPtr*) buffer));
                        }
                    }
                    else
                    {
                        ((SafeFreeCertContext) refHandle).Set(*((IntPtr*) buffer));
                    }
                }
                if ((num != 0) && (refHandle != null))
                {
                    refHandle.SetHandleAsInvalid();
                }
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern unsafe int QueryContextAttributesW(ref SSPIHandle contextHandle, [In] ContextAttribute attribute, [In] void* buffer);
        protected override bool ReleaseHandle()
        {
            return (FreeContextBuffer(base.handle) == 0);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void Set(IntPtr value)
        {
            base.handle = value;
        }
    }
}

