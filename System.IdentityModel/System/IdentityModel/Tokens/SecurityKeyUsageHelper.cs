namespace System.IdentityModel.Tokens
{
    using System;
    using System.ComponentModel;
    using System.IdentityModel;

    internal static class SecurityKeyUsageHelper
    {
        internal static bool IsDefined(SecurityKeyUsage value)
        {
            if (value != SecurityKeyUsage.Exchange)
            {
                return (value == SecurityKeyUsage.Signature);
            }
            return true;
        }

        internal static void Validate(SecurityKeyUsage value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(SecurityKeyUsage)));
            }
        }
    }
}

