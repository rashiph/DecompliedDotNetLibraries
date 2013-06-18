namespace System.ServiceModel.MsmqIntegration
{
    using System;

    internal static class MsmqIntegrationSecurityModeHelper
    {
        internal static bool IsDefined(MsmqIntegrationSecurityMode value)
        {
            if (value != MsmqIntegrationSecurityMode.Transport)
            {
                return (value == MsmqIntegrationSecurityMode.None);
            }
            return true;
        }
    }
}

