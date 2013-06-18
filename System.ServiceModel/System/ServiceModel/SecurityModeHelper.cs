namespace System.ServiceModel
{
    using System;

    internal static class SecurityModeHelper
    {
        internal static bool IsDefined(SecurityMode value)
        {
            if (((value != SecurityMode.None) && (value != SecurityMode.Transport)) && (value != SecurityMode.Message))
            {
                return (value == SecurityMode.TransportWithMessageCredential);
            }
            return true;
        }

        internal static SecurityMode ToSecurityMode(UnifiedSecurityMode value)
        {
            switch (value)
            {
                case UnifiedSecurityMode.None:
                    return SecurityMode.None;

                case UnifiedSecurityMode.Transport:
                    return SecurityMode.Transport;

                case UnifiedSecurityMode.Message:
                    return SecurityMode.Message;

                case UnifiedSecurityMode.TransportWithMessageCredential:
                    return SecurityMode.TransportWithMessageCredential;
            }
            return (SecurityMode) value;
        }
    }
}

