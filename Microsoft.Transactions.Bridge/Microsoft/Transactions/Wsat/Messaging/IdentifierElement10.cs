namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;
    using System.Xml.Serialization;

    [XmlRoot(ElementName="Identifier", Namespace="http://schemas.xmlsoap.org/ws/2004/10/wscoor")]
    internal class IdentifierElement10 : IdentifierElement
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IdentifierElement10() : this(null)
        {
        }

        public IdentifierElement10(string identifier) : base(ProtocolVersion.Version10, identifier)
        {
        }
    }
}

