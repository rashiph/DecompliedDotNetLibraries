namespace System.ServiceModel.Security
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;

    internal sealed class SecurityKeyEntropyModeHelper
    {
        internal static bool IsDefined(SecurityKeyEntropyMode value)
        {
            if ((value != SecurityKeyEntropyMode.ClientEntropy) && (value != SecurityKeyEntropyMode.ServerEntropy))
            {
                return (value == SecurityKeyEntropyMode.CombinedEntropy);
            }
            return true;
        }

        internal static void Validate(SecurityKeyEntropyMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(SecurityKeyEntropyMode)));
            }
        }
    }
}

