namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;

    internal static class ProtocolVersionHelper
    {
        public static void AssertProtocolVersion(ProtocolVersion protocolVersion, Type type, string method)
        {
            if (!Enum.IsDefined(typeof(ProtocolVersion), protocolVersion))
            {
                DiagnosticUtility.FailFast(string.Concat(new object[] { "An invalid protocol version value was used in ", type, '.', method }));
            }
        }

        public static void AssertProtocolVersion10(ProtocolVersion protocolVersion, Type type, string method)
        {
            if (protocolVersion != ProtocolVersion.Version10)
            {
                DiagnosticUtility.FailFast(string.Concat(new object[] { "Must use the protocol version 1.0 to execute ", type, '.', method }));
            }
        }

        public static void AssertProtocolVersion11(ProtocolVersion protocolVersion, Type type, string method)
        {
            if (protocolVersion != ProtocolVersion.Version11)
            {
                DiagnosticUtility.FailFast(string.Concat(new object[] { "Must use the protocol version 1.1 to execute ", type, '.', method }));
            }
        }
    }
}

