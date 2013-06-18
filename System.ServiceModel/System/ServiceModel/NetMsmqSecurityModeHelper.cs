namespace System.ServiceModel
{
    using System;

    internal static class NetMsmqSecurityModeHelper
    {
        internal static bool IsDefined(NetMsmqSecurityMode value)
        {
            if (((value != NetMsmqSecurityMode.Transport) && (value != NetMsmqSecurityMode.Message)) && (value != NetMsmqSecurityMode.Both))
            {
                return (value == NetMsmqSecurityMode.None);
            }
            return true;
        }

        internal static NetMsmqSecurityMode ToSecurityMode(UnifiedSecurityMode value)
        {
            switch (value)
            {
                case UnifiedSecurityMode.None:
                    return NetMsmqSecurityMode.None;

                case UnifiedSecurityMode.Transport:
                    return NetMsmqSecurityMode.Transport;

                case UnifiedSecurityMode.Message:
                    return NetMsmqSecurityMode.Message;

                case UnifiedSecurityMode.Both:
                    return NetMsmqSecurityMode.Both;
            }
            return (NetMsmqSecurityMode) value;
        }
    }
}

