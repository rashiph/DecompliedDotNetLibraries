namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;
    using System.Xml.Serialization;

    [XmlRoot(ElementName="Identifier", Namespace="http://docs.oasis-open.org/ws-tx/wscoor/2006/06")]
    internal class IdentifierElement11 : IdentifierElement
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IdentifierElement11() : this(null)
        {
        }

        public IdentifierElement11(string identifier) : base(ProtocolVersion.Version11, identifier)
        {
        }
    }
}

