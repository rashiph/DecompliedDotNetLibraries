namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal static class NetTcpDefaults
    {
        internal const MessageCredentialType MessageSecurityClientCredentialType = MessageCredentialType.Windows;
        internal const bool TransactionsEnabled = false;

        internal static SecurityAlgorithmSuite MessageSecurityAlgorithmSuite
        {
            get
            {
                return SecurityAlgorithmSuite.Default;
            }
        }

        internal static System.ServiceModel.TransactionProtocol TransactionProtocol
        {
            get
            {
                return System.ServiceModel.TransactionProtocol.Default;
            }
        }
    }
}

