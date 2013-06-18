namespace System.ServiceModel.Channels
{
    using System;

    internal static class TransactionPolicyStrings
    {
        public const string OleTxTransactionsLocal = "OleTxAssertion";
        public const string OleTxTransactionsNamespace = "http://schemas.microsoft.com/ws/2006/02/tx/oletx";
        public const string OleTxTransactionsPrefix = "oletx";
        public const string OptionalLocal = "Optional";
        public const string OptionalNamespaceLegacy = "http://schemas.xmlsoap.org/ws/2002/12/policy";
        public const string OptionalPrefix10 = "wsp1";
        public const string OptionalPrefix11 = "wsp";
        public const string TrueValue = "true";
        public const string WsatTransactionsLocal = "ATAssertion";
        public const string WsatTransactionsNamespace10 = "http://schemas.xmlsoap.org/ws/2004/10/wsat";
        public const string WsatTransactionsNamespace11 = "http://docs.oasis-open.org/ws-tx/wsat/2006/06";
        public const string WsatTransactionsPrefix = "wsat";
    }
}

