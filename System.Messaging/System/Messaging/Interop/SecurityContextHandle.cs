namespace System.Messaging.Interop
{
    using Microsoft.Win32.SafeHandles;
    using System;

    internal sealed class SecurityContextHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SecurityContextHandle(IntPtr existingHandle) : base(true)
        {
            base.SetHandle(existingHandle);
        }

        protected override bool ReleaseHandle()
        {
            SafeNativeMethods.MQFreeSecurityContext(base.handle);
            return true;
        }

        public override bool IsInvalid
        {
            get
            {
                if (!base.IsInvalid)
                {
                    return base.IsClosed;
                }
                return true;
            }
        }
    }
}

