namespace System.ServiceModel.Description
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;

    internal static class MetadataExchangeClientModeHelper
    {
        public static bool IsDefined(MetadataExchangeClientMode x)
        {
            if (x != MetadataExchangeClientMode.MetadataExchange)
            {
                return (x == MetadataExchangeClientMode.HttpGet);
            }
            return true;
        }

        public static void Validate(MetadataExchangeClientMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(MetadataExchangeClientMode)));
            }
        }
    }
}

