namespace System.Net
{
    using System;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;

    [SuppressUnmanagedCodeSecurity]
    internal class SafeLocalFreeChannelBinding : ChannelBinding
    {
        private const int LMEM_FIXED = 0;
        private int size;

        public static SafeLocalFreeChannelBinding LocalAlloc(int cb)
        {
            SafeLocalFreeChannelBinding binding = UnsafeNclNativeMethods.SafeNetHandles.LocalAllocChannelBinding(0, (UIntPtr) cb);
            if (binding.IsInvalid)
            {
                binding.SetHandleAsInvalid();
                throw new OutOfMemoryException();
            }
            binding.size = cb;
            return binding;
        }

        protected override bool ReleaseHandle()
        {
            return (UnsafeNclNativeMethods.SafeNetHandles.LocalFree(base.handle) == IntPtr.Zero);
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

