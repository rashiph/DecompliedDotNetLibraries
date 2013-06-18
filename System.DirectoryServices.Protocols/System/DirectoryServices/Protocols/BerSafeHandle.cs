namespace System.DirectoryServices.Protocols
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class BerSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal BerSafeHandle() : base(true)
        {
            base.SetHandle(Wldap32.ber_alloc(1));
            if (base.handle == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }
        }

        internal BerSafeHandle(berval value) : base(true)
        {
            base.SetHandle(Wldap32.ber_init(value));
            if (base.handle == IntPtr.Zero)
            {
                throw new BerConversionException();
            }
        }

        protected override bool ReleaseHandle()
        {
            Wldap32.ber_free(base.handle, 1);
            return true;
        }
    }
}

