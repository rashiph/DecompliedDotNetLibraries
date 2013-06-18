namespace Microsoft.Transactions.Bridge
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    internal class TransactionBridgeException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TransactionBridgeException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TransactionBridgeException(string exception) : base(exception)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TransactionBridgeException(SerializationInfo serInfo, StreamingContext streaming) : base(serInfo, streaming)
        {
        }
    }
}

