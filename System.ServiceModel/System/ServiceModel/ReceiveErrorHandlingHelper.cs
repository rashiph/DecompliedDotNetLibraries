namespace System.ServiceModel
{
    using System;

    internal static class ReceiveErrorHandlingHelper
    {
        internal static bool IsDefined(ReceiveErrorHandling value)
        {
            if (((value != ReceiveErrorHandling.Fault) && (value != ReceiveErrorHandling.Drop)) && (value != ReceiveErrorHandling.Reject))
            {
                return (value == ReceiveErrorHandling.Move);
            }
            return true;
        }
    }
}

