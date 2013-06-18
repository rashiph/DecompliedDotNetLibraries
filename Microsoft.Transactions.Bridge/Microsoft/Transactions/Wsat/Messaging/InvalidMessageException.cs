namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class InvalidMessageException : CommunicationException
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InvalidMessageException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InvalidMessageException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

