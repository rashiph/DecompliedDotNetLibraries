namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal abstract class Proxy : ReferenceCountedObject
    {
        protected AtomicTransactionStrings atomicTransactionStrings;
        protected CoordinationService coordinationService;
        protected CoordinationStrings coordinationStrings;
        protected EndpointAddress from;
        protected System.ServiceModel.Channels.MessageVersion messageVersion;
        protected Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion;
        protected EndpointAddress to;

        protected Proxy(CoordinationService coordination, EndpointAddress to, EndpointAddress from)
        {
            this.coordinationService = coordination;
            this.to = to;
            this.from = from;
            this.protocolVersion = coordination.ProtocolVersion;
            this.coordinationStrings = CoordinationStrings.Version(coordination.ProtocolVersion);
            this.atomicTransactionStrings = AtomicTransactionStrings.Version(coordination.ProtocolVersion);
        }

        public EndpointAddress From
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.from;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.from = value;
            }
        }

        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.messageVersion;
            }
        }

        public Microsoft.Transactions.Wsat.Protocol.ProtocolVersion ProtocolVersion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolVersion;
            }
        }

        public EndpointAddress To
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.to;
            }
        }
    }
}

