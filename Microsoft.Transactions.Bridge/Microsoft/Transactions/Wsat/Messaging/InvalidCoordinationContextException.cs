namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class InvalidCoordinationContextException : CommunicationException
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InvalidCoordinationContextException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InvalidCoordinationContextException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

