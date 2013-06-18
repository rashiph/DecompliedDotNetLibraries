namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;

    internal class MessagingInitializationException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MessagingInitializationException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MessagingInitializationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

