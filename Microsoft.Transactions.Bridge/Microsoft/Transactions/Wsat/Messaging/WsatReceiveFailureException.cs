namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;

    internal class WsatReceiveFailureException : WsatMessagingException
    {
        public WsatReceiveFailureException(Exception inner) : base(inner.Message, inner)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WsatReceiveFailureException(string message) : base(message)
        {
        }
    }
}

