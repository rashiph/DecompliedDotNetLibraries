namespace System.Web.Services
{
    using System;

    internal sealed class Soap12
    {
        internal const string Encoding = "http://www.w3.org/2003/05/soap-encoding";
        internal const string Namespace = "http://www.w3.org/2003/05/soap-envelope";
        internal const string Prefix = "soap12";
        internal const string RpcNamespace = "http://www.w3.org/2003/05/soap-rpc";

        private Soap12()
        {
        }

        internal class Attribute
        {
            internal const string Relay = "relay";
            internal const string Role = "role";
            internal const string UpgradeEnvelopeQname = "qname";

            private Attribute()
            {
            }
        }

        internal sealed class Code
        {
            internal const string DataEncodingUnknown = "DataEncodingUnknown";
            internal const string EncodingMissingIDFaultSubcode = "MissingID";
            internal const string EncodingUntypedValueFaultSubcode = "UntypedValue";
            internal const string MustUnderstand = "MustUnderstand";
            internal const string Receiver = "Receiver";
            internal const string RpcBadArgumentsSubcode = "BadArguments";
            internal const string RpcProcedureNotPresentSubcode = "ProcedureNotPresent";
            internal const string Sender = "Sender";
            internal const string VersionMismatch = "VersionMismatch";

            private Code()
            {
            }
        }

        internal sealed class Element
        {
            internal const string FaultCode = "Code";
            internal const string FaultCodeValue = "Value";
            internal const string FaultDetail = "Detail";
            internal const string FaultNode = "Node";
            internal const string FaultReason = "Reason";
            internal const string FaultReasonText = "Text";
            internal const string FaultRole = "Role";
            internal const string FaultSubcode = "Subcode";
            internal const string Upgrade = "Upgrade";
            internal const string UpgradeEnvelope = "SupportedEnvelope";

            private Element()
            {
            }
        }
    }
}

