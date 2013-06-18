namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;

    internal class CreateChannelFailureException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CreateChannelFailureException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CreateChannelFailureException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

