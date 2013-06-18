namespace System.ServiceModel
{
    using System;

    internal static class BasicHttpSecurityModeHelper
    {
        internal static bool IsDefined(BasicHttpSecurityMode value)
        {
            if (((value != BasicHttpSecurityMode.None) && (value != BasicHttpSecurityMode.Transport)) && ((value != BasicHttpSecurityMode.Message) && (value != BasicHttpSecurityMode.TransportWithMessageCredential)))
            {
                return (value == BasicHttpSecurityMode.TransportCredentialOnly);
            }
            return true;
        }

        internal static BasicHttpSecurityMode ToSecurityMode(UnifiedSecurityMode value)
        {
            switch (value)
            {
                case UnifiedSecurityMode.None:
                    return BasicHttpSecurityMode.None;

                case UnifiedSecurityMode.Transport:
                    return BasicHttpSecurityMode.Transport;

                case UnifiedSecurityMode.Message:
                    return BasicHttpSecurityMode.Message;

                case UnifiedSecurityMode.TransportWithMessageCredential:
                    return BasicHttpSecurityMode.TransportWithMessageCredential;

                case UnifiedSecurityMode.TransportCredentialOnly:
                    return BasicHttpSecurityMode.TransportCredentialOnly;
            }
            return (BasicHttpSecurityMode) value;
        }
    }
}

