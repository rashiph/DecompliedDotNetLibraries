namespace System.Net
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;

    [SuppressUnmanagedCodeSecurity]
    internal abstract class SafeFreeContextBufferChannelBinding : ChannelBinding
    {
        private int size;

        protected SafeFreeContextBufferChannelBinding()
        {
        }

        internal static SafeFreeContextBufferChannelBinding CreateEmptyHandle(SecurDll dll)
        {
            switch (dll)
            {
                case SecurDll.SECURITY:
                    return new SafeFreeContextBufferChannelBinding_SECURITY();

                case SecurDll.SECUR32:
                    return new SafeFreeContextBufferChannelBinding_SECUR32();

                case SecurDll.SCHANNEL:
                    return new SafeFreeContextBufferChannelBinding_SCHANNEL();
            }
            throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "SecurDll" }), "dll");
        }

        public static unsafe int QueryContextChannelBinding(SecurDll dll, SafeDeleteContext phContext, ContextAttribute contextAttribute, Bindings* buffer, SafeFreeContextBufferChannelBinding refHandle)
        {
            switch (dll)
            {
                case SecurDll.SECURITY:
                    return QueryContextChannelBinding_SECURITY(phContext, contextAttribute, buffer, refHandle);

                case SecurDll.SECUR32:
                    return QueryContextChannelBinding_SECUR32(phContext, contextAttribute, buffer, refHandle);

                case SecurDll.SCHANNEL:
                    return QueryContextChannelBinding_SCHANNEL(phContext, contextAttribute, buffer, refHandle);
            }
            return -1;
        }

        private static unsafe int QueryContextChannelBinding_SCHANNEL(SafeDeleteContext phContext, ContextAttribute contextAttribute, Bindings* buffer, SafeFreeContextBufferChannelBinding refHandle)
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
                    refHandle.Set(buffer.pBindings);
                    refHandle.size = buffer.BindingsLength;
                }
                if ((num != 0) && (refHandle != null))
                {
                    refHandle.SetHandleAsInvalid();
                }
            }
            return num;
        }

        private static unsafe int QueryContextChannelBinding_SECUR32(SafeDeleteContext phContext, ContextAttribute contextAttribute, Bindings* buffer, SafeFreeContextBufferChannelBinding refHandle)
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
                    refHandle.Set(buffer.pBindings);
                    refHandle.size = buffer.BindingsLength;
                }
                if ((num != 0) && (refHandle != null))
                {
                    refHandle.SetHandleAsInvalid();
                }
            }
            return num;
        }

        private static unsafe int QueryContextChannelBinding_SECURITY(SafeDeleteContext phContext, ContextAttribute contextAttribute, Bindings* buffer, SafeFreeContextBufferChannelBinding refHandle)
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
                    refHandle.Set(buffer.pBindings);
                    refHandle.size = buffer.BindingsLength;
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

        public override int Size
        {
            get
            {
                return this.size;
            }
        }
    }
}

