namespace System.ServiceModel.Channels
{
    using System;

    internal static class SupportedAddressingModeHelper
    {
        internal static bool IsDefined(SupportedAddressingMode value)
        {
            if ((value != SupportedAddressingMode.Anonymous) && (value != SupportedAddressingMode.NonAnonymous))
            {
                return (value == SupportedAddressingMode.Mixed);
            }
            return true;
        }
    }
}

