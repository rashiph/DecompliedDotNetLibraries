namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal static class SecurityStandardsHelper
    {
        private static SecurityStandardsManager SecurityStandardsManager2007 = CreateStandardsManager(MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12);

        private static SecurityStandardsManager CreateStandardsManager(MessageSecurityVersion securityVersion)
        {
            return new SecurityStandardsManager(securityVersion, new WSSecurityTokenSerializer(securityVersion.SecurityVersion, securityVersion.TrustVersion, securityVersion.SecureConversationVersion, false, null, null, null));
        }

        public static SecurityStandardsManager CreateStandardsManager(TransactionProtocol transactionProtocol)
        {
            if ((transactionProtocol != TransactionProtocol.WSAtomicTransactionOctober2004) && (transactionProtocol != TransactionProtocol.OleTransactions))
            {
                return SecurityStandardsManager2007;
            }
            return SecurityStandardsManager.DefaultInstance;
        }
    }
}

