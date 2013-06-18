namespace System.Messaging.Interop
{
    using Microsoft.Win32.SafeHandles;
    using System;

    internal class CursorHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public static readonly CursorHandle NullHandle = new InvalidCursorHandle();

        protected CursorHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            SafeNativeMethods.MQCloseCursor(base.handle);
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

        private sealed class InvalidCursorHandle : CursorHandle
        {
            protected override bool ReleaseHandle()
            {
                return true;
            }
        }
    }
}

