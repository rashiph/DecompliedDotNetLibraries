namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class InvalidEnlistmentHeaderException : CommunicationException
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InvalidEnlistmentHeaderException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InvalidEnlistmentHeaderException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

