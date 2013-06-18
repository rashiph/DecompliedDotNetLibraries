namespace System.Messaging.Interop
{
    using Microsoft.Win32.SafeHandles;
    using System;

    internal class MessageQueueHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public static readonly MessageQueueHandle InvalidHandle = new InvalidMessageQueueHandle();

        private MessageQueueHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            SafeNativeMethods.MQCloseQueue(base.handle);
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

        private sealed class InvalidMessageQueueHandle : MessageQueueHandle
        {
            protected override bool ReleaseHandle()
            {
                return true;
            }
        }
    }
}

