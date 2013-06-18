namespace System.ServiceModel
{
    using System;

    internal static class WSDualHttpSecurityModeHelper
    {
        internal static bool IsDefined(WSDualHttpSecurityMode value)
        {
            if (value != WSDualHttpSecurityMode.None)
            {
                return (value == WSDualHttpSecurityMode.Message);
            }
            return true;
        }
    }
}

