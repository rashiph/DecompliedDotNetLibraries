namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="RegistrationCoordinatorFailed")]
    internal class RegistrationCoordinatorFailedSchema : TraceRecord
    {
        [DataMember(Name="Context", IsRequired=true)]
        private CoordinationContext context;
        private ControlProtocol protocol;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RegistrationCoordinatorFailedTraceRecord";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RegistrationCoordinatorFailedSchema(CoordinationContext context, ControlProtocol protocol)
        {
            this.context = context;
            this.protocol = protocol;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            TransactionTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/RegistrationCoordinatorFailedTraceRecord";
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

