namespace System.ServiceModel.Channels
{
    using System;

    internal static class ContextExchangeMechanismHelper
    {
        public static bool IsDefined(ContextExchangeMechanism value)
        {
            if (value != ContextExchangeMechanism.ContextSoapHeader)
            {
                return (value == ContextExchangeMechanism.HttpCookie);
            }
            return true;
        }
    }
}

