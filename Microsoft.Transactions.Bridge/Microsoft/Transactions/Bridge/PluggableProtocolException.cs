namespace Microsoft.Transactions.Bridge
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    internal class PluggableProtocolException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PluggableProtocolException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PluggableProtocolException(string exception) : base(exception)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected PluggableProtocolException(SerializationInfo serInfo, StreamingContext streaming) : base(serInfo, streaming)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PluggableProtocolException(string exception, Exception e) : base(exception, e)
        {
        }
    }
}

