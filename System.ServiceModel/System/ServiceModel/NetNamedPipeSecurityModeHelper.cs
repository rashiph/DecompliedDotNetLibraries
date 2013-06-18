namespace System.ServiceModel
{
    using System;

    internal static class NetNamedPipeSecurityModeHelper
    {
        internal static bool IsDefined(NetNamedPipeSecurityMode value)
        {
            if (value != NetNamedPipeSecurityMode.Transport)
            {
                return (value == NetNamedPipeSecurityMode.None);
            }
            return true;
        }
    }
}

