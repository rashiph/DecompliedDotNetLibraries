namespace Microsoft.Transactions.Wsat.Recovery
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    internal class SerializationException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SerializationException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SerializationException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected SerializationException(SerializationInfo serInfo, StreamingContext streaming) : base(serInfo, streaming)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SerializationException(string message, Exception e) : base(message, e)
        {
        }
    }
}

