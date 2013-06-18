namespace System.ServiceModel
{
    using System;

    internal static class WSFederationHttpSecurityModeHelper
    {
        internal static bool IsDefined(WSFederationHttpSecurityMode value)
        {
            if ((value != WSFederationHttpSecurityMode.None) && (value != WSFederationHttpSecurityMode.Message))
            {
                return (value == WSFederationHttpSecurityMode.TransportWithMessageCredential);
            }
            return true;
        }
    }
}

