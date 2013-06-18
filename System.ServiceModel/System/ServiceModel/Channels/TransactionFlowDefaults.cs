namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal static class TransactionFlowDefaults
    {
        internal const TransactionFlowOption IssuedTokens = TransactionFlowOption.NotAllowed;
        internal static System.ServiceModel.TransactionProtocol TransactionProtocol = System.ServiceModel.TransactionProtocol.OleTransactions;
        internal const string TransactionProtocolString = "OleTransactions";
        internal const bool Transactions = false;
    }
}

