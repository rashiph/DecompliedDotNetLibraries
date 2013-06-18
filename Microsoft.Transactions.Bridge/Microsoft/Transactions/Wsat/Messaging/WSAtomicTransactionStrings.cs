namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal static class WSAtomicTransactionStrings
    {
        public static XmlDictionaryString ProtocolToWellKnownName(ControlProtocol ctrlProt, ProtocolVersion protocolVersion)
        {
            AtomicTransactionXmlDictionaryStrings strings = AtomicTransactionXmlDictionaryStrings.Version(protocolVersion);
            switch (ctrlProt)
            {
                case ControlProtocol.Completion:
                    return strings.CompletionUri;

                case ControlProtocol.Volatile2PC:
                    return strings.Volatile2PCUri;

                case ControlProtocol.Durable2PC:
                    return strings.Durable2PCUri;
            }
            DiagnosticUtility.FailFast("Invalid protocol");
            return null;
        }

        public static ControlProtocol WellKnownNameToProtocol(string name, ProtocolVersion protocolVersion)
        {
            AtomicTransactionStrings strings = AtomicTransactionStrings.Version(protocolVersion);
            if (name == strings.CompletionUri)
            {
                return ControlProtocol.Completion;
            }
            if (name == strings.Durable2PCUri)
            {
                return ControlProtocol.Durable2PC;
            }
            if (name == strings.Volatile2PCUri)
            {
                return ControlProtocol.Volatile2PC;
            }
            return ControlProtocol.None;
        }
    }
}

