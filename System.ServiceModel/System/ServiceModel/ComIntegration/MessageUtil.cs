namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.EnterpriseServices;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Transactions;

    internal static class MessageUtil
    {
        public static WindowsIdentity GetMessageIdentity(Message message)
        {
            WindowsIdentity windowsIdentity = null;
            SecurityMessageProperty security = message.Properties.Security;
            if (security != null)
            {
                ServiceSecurityContext serviceSecurityContext = security.ServiceSecurityContext;
                if (serviceSecurityContext != null)
                {
                    if (serviceSecurityContext.WindowsIdentity == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.RequiresWindowsSecurity());
                    }
                    windowsIdentity = serviceSecurityContext.WindowsIdentity;
                }
            }
            if ((windowsIdentity != null) && !windowsIdentity.IsAnonymous)
            {
                return windowsIdentity;
            }
            return System.ServiceModel.ComIntegration.SecurityUtils.GetAnonymousIdentity();
        }

        public static Transaction GetMessageTransaction(Message message)
        {
            Transaction transaction;
            ServiceConfig cfg = new ServiceConfig {
                Transaction = TransactionOption.Disabled
            };
            ServiceDomain.Enter(cfg);
            try
            {
                transaction = TransactionMessageProperty.TryGetTransaction(message);
            }
            finally
            {
                ServiceDomain.Leave();
            }
            return transaction;
        }
    }
}

