namespace System.Data
{
    using System;
    using System.Data.Common;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class OperationAbortedException : SystemException
    {
        private OperationAbortedException(SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
        }

        private OperationAbortedException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146232010;
        }

        internal static OperationAbortedException Aborted(Exception inner)
        {
            OperationAbortedException exception;
            if (inner == null)
            {
                exception = new OperationAbortedException(Res.GetString("ADP_OperationAborted"), null);
            }
            else
            {
                exception = new OperationAbortedException(Res.GetString("ADP_OperationAbortedExceptionMessage"), inner);
            }
            ADP.TraceExceptionAsReturnValue(exception);
            return exception;
        }
    }
}

