namespace System.Drawing.Drawing2D
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal class SafeCustomLineCapHandle : SafeHandle
    {
        internal SafeCustomLineCapHandle(IntPtr h) : base(IntPtr.Zero, true)
        {
            base.SetHandle(h);
        }

        public static explicit operator SafeCustomLineCapHandle(IntPtr handle)
        {
            return new SafeCustomLineCapHandle(handle);
        }

        public static implicit operator IntPtr(SafeCustomLineCapHandle handle)
        {
            if (handle != null)
            {
                return handle.handle;
            }
            return IntPtr.Zero;
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            int num = 0;
            if (!this.IsInvalid)
            {
                try
                {
                    num = SafeNativeMethods.Gdip.GdipDeleteCustomLineCap(new HandleRef(this, base.handle));
                }
                catch (Exception exception)
                {
                    if (System.Drawing.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                finally
                {
                    base.handle = IntPtr.Zero;
                }
            }
            return (num != 0);
        }

        public override bool IsInvalid
        {
            get
            {
                return (base.handle == IntPtr.Zero);
            }
        }
    }
}

