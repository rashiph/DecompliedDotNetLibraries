namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Xml;

    [DataContract(Name="RegistrationCoordinatorResponseInvalidMetadata")]
    internal abstract class RegistrationCoordinatorResponseInvalidMetadataSchema : TraceRecord
    {
        [DataMember(Name="Context", IsRequired=true)]
        private CoordinationContext context;
        private ControlProtocol protocol;
        protected string schemaId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected RegistrationCoordinatorResponseInvalidMetadataSchema(CoordinationContext context, ControlProtocol protocol)
        {
            this.context = context;
            this.protocol = protocol;
        }

        public static RegistrationCoordinatorResponseInvalidMetadataSchema Instance(CoordinationContext context, ControlProtocol protocol, EndpointAddress coordinatorService, ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(RegistrationCoordinatorResponseInvalidMetadataSchema), "Instance");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return new RegistrationCoordinatorResponseInvalidMetadataSchema10(context, protocol, coordinatorService);

                case ProtocolVersion.Version11:
                    return new RegistrationCoordinatorResponseInvalidMetadataSchema11(context, protocol, coordinatorService);
            }
            return null;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            TransactionTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.schemaId;
            }
        }

        [DataMember(Name="Protocol", IsRequired=true)]
        private string Protocol
        {
            get
            {
                return this.protocol.ToString();
            }
            set
            {
            }
        }
    }
}

