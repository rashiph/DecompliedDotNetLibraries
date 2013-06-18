namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;

    internal abstract class WsatMessagingException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected WsatMessagingException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected WsatMessagingException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

