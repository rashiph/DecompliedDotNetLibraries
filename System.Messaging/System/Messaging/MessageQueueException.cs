namespace System.Messaging
{
    using System;
    using System.Globalization;
    using System.Messaging.Interop;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable]
    public class MessageQueueException : ExternalException, ISerializable
    {
        private readonly int nativeErrorCode;

        internal MessageQueueException(int error)
        {
            this.nativeErrorCode = error;
        }

        protected MessageQueueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.nativeErrorCode = info.GetInt32("NativeErrorCode");
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("NativeErrorCode", this.nativeErrorCode);
            base.GetObjectData(info, context);
        }

        private static string GetUnknownErrorMessage(int error)
        {
            StringBuilder lpBuffer = new StringBuilder(0x100);
            if (SafeNativeMethods.FormatMessage(0x3200, IntPtr.Zero, error, 0, lpBuffer, lpBuffer.Capacity + 1, IntPtr.Zero) == 0)
            {
                return Res.GetString("UnknownError", new object[] { Convert.ToString(error, 0x10) });
            }
            int length = lpBuffer.Length;
            while (length > 0)
            {
                char ch = lpBuffer[length - 1];
                if ((ch > ' ') && (ch != '.'))
                {
                    break;
                }
                length--;
            }
            return lpBuffer.ToString(0, length);
        }

        public override string Message
        {
            get
            {
                try
                {
                    return Res.GetString(Convert.ToString(this.nativeErrorCode, 0x10).ToUpper(CultureInfo.InvariantCulture));
                }
                catch
                {
                    return GetUnknownErrorMessage(this.nativeErrorCode);
                }
            }
        }

        public System.Messaging.MessageQueueErrorCode MessageQueueErrorCode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return (System.Messaging.MessageQueueErrorCode) this.nativeErrorCode;
            }
        }
    }
}

