namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using System;
    using System.Runtime;
    using System.ServiceModel.Channels;

    internal class WsatFaultException : WsatMessagingException
    {
        private string action;
        private MessageFault fault;

        public WsatFaultException(MessageFault fault, string action) : base(GetExceptionMessage(fault))
        {
            this.fault = fault;
            this.action = action;
        }

        private static string GetExceptionMessage(MessageFault fault)
        {
            return Microsoft.Transactions.SR.GetString("RequestReplyFault", new object[] { Library.GetFaultCodeName(fault), Library.GetFaultCodeReason(fault) });
        }

        public string Action
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.action;
            }
        }

        public MessageFault Fault
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.fault;
            }
        }
    }
}

