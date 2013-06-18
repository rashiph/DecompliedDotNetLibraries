namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal abstract class TwoPhaseCommitParticipantEvent : ParticipantEvent
    {
        private EndpointAddress faultTo;
        private UniqueId messageID;
        private EndpointAddress replyTo;

        public TwoPhaseCommitParticipantEvent(ParticipantEnlistment participant) : base(participant)
        {
            MessageHeaders incomingMessageHeaders = OperationContext.Current.IncomingMessageHeaders;
            this.faultTo = Library.GetFaultToHeader(incomingMessageHeaders, base.state.ProtocolVersion);
            this.replyTo = Library.GetReplyToHeader(incomingMessageHeaders);
            this.messageID = incomingMessageHeaders.MessageId;
        }

        public EndpointAddress FaultTo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.faultTo;
            }
        }

        public UniqueId MessageId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.messageID;
            }
        }

        public EndpointAddress ReplyTo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.replyTo;
            }
        }
    }
}

