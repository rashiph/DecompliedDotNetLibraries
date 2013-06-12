namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public sealed class ThreadAbortException : SystemException
    {
        private ThreadAbortException() : base(Exception.GetMessageFromNativeResources(Exception.ExceptionMessageKind.ThreadAbort))
        {
            base.SetErrorCode(-2146233040);
        }

        internal ThreadAbortException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public object ExceptionState
        {
            [SecuritySafeCritical]
            get
            {
                return Thread.CurrentThread.AbortReason;
            }
        }
    }
}

