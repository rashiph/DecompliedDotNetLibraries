namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Transactions;

    [Serializable]
    internal sealed class TransactionProxyException : COMException
    {
        private TransactionProxyException(int hr, TransactionException exception) : base(null, exception)
        {
            base.HResult = hr;
        }

        private TransactionProxyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public static void ThrowTransactionProxyException(int hr, TransactionException exception)
        {
            throw new TransactionProxyException(hr, exception);
        }
    }
}

