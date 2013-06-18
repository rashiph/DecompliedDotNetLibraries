namespace System.Messaging.Interop
{
    using Microsoft.Win32.SafeHandles;
    using System;

    internal class LocatorHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public static readonly LocatorHandle InvalidHandle = new InvalidLocatorHandle();

        protected LocatorHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            SafeNativeMethods.MQLocateEnd(base.handle);
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

        private sealed class InvalidLocatorHandle : LocatorHandle
        {
            protected override bool ReleaseHandle()
            {
                return true;
            }
        }
    }
}

