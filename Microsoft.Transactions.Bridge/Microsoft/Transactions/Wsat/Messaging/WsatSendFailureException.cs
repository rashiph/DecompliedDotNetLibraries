namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;

    internal class WsatSendFailureException : WsatMessagingException
    {
        public WsatSendFailureException(Exception inner) : base(inner.Message, inner)
        {
        }
    }
}

