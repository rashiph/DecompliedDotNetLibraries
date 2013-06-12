namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal abstract class SafeFreeContextBuffer : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected SafeFreeContextBuffer() : base(true)
        {
        }

        internal static SafeFreeContextBuffer CreateEmptyHandle(SecurDll dll)
        {
            switch (dll)
            {
                case SecurDll.SECURITY:
                    return new SafeFreeContextBuffer_SECURITY();

                case SecurDll.SECUR32:
                    return new SafeFreeContextBuffer_SECUR32();

                case SecurDll.SCHANNEL:
                    return new SafeFreeContextBuffer_SCHANNEL();
            }
            throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "SecurDll" }), "dll");
        }

        internal static int EnumeratePackages(SecurDll Dll, out int pkgnum, out SafeFreeContextBuffer pkgArray)
        {
            int num = -1;
            switch (Dll)
            {
                case SecurDll.SECURITY:
                {
                    SafeFreeContextBuffer_SECURITY handle = null;
                    num = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.EnumerateSecurityPackagesW(out pkgnum, out handle);
                    pkgArray = handle;
                    break;
                }
                case SecurDll.SECUR32:
                {
                    SafeFreeContextBuffer_SECUR32 r_secur = null;
                    num = UnsafeNclNativeMethods.SafeNetHandles_SECUR32.EnumerateSecurityPackagesA(out pkgnum, out r_secur);
                    pkgArray = r_secur;
                    break;
                }
                case SecurDll.SCHANNEL:
                {
                    SafeFreeContextBuffer_SCHANNEL r_schannel = null;
                    num = UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.EnumerateSecurityPackagesA(out pkgnum, out r_schannel);
                    pkgArray = r_schannel;
                    break;
                }
                default:
                    throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "SecurDll" }), "Dll");
            }
            if ((num != 0) && (pkgArray != null))
            {
                pkgArray.SetHandleAsInvalid();
            }
            return num;
        }

        public static unsafe int QueryContextAttributes(SecurDll dll, SafeDeleteContext phContext, ContextAttribute contextAttribute, byte* buffer, SafeHandle refHandle)
        {
            switch (dll)
            {
                case SecurDll.SECURITY:
                    return QueryContextAttributes_SECURITY(phContext, contextAttribute, buffer, refHandle);

                case SecurDll.SECUR32:
                    return QueryContextAttributes_SECUR32(phContext, contextAttribute, buffer, refHandle);

                case SecurDll.SCHANNEL:
                    return QueryContextAttributes_SCHANNEL(phContext, contextAttribute, buffer, refHandle);
            }
            return -1;
        }

        private static unsafe int QueryContextAttributes_SCHANNEL(SafeDeleteContext phContext, ContextAttribute contextAttribute, byte* buffer, SafeHandle refHandle)
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
                    num = UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.QueryContextAttributesA(ref phContext._handle, contextAttribute, (void*) buffer);
                    phContext.DangerousRelease();
                }
                if ((num == 0) && (refHandle != null))
                {
                    if (refHandle is SafeFreeContextBuffer)
                    {
                        ((SafeFreeContextBuffer) refHandle).Set(*((IntPtr*) buffer));
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

        private static unsafe int QueryContextAttributes_SECUR32(SafeDeleteContext phContext, ContextAttribute contextAttribute, byte* buffer, SafeHandle refHandle)
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
                    num = UnsafeNclNativeMethods.SafeNetHandles_SECUR32.QueryContextAttributesA(ref phContext._handle, contextAttribute, (void*) buffer);
                    phContext.DangerousRelease();
                }
                if ((num == 0) && (refHandle != null))
                {
                    if (refHandle is SafeFreeContextBuffer)
                    {
                        ((SafeFreeContextBuffer) refHandle).Set(*((IntPtr*) buffer));
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

        private static unsafe int QueryContextAttributes_SECURITY(SafeDeleteContext phContext, ContextAttribute contextAttribute, byte* buffer, SafeHandle refHandle)
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
                    num = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.QueryContextAttributesW(ref phContext._handle, contextAttribute, (void*) buffer);
                    phContext.DangerousRelease();
                }
                if ((num == 0) && (refHandle != null))
                {
                    if (refHandle is SafeFreeContextBuffer)
                    {
                        ((SafeFreeContextBuffer) refHandle).Set(*((IntPtr*) buffer));
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

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void Set(IntPtr value)
        {
            base.handle = value;
        }
    }
}

