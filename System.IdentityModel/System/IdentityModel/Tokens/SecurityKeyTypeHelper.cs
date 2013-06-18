namespace System.IdentityModel.Tokens
{
    using System;
    using System.ComponentModel;
    using System.IdentityModel;

    internal static class SecurityKeyTypeHelper
    {
        internal static bool IsDefined(SecurityKeyType value)
        {
            if ((value != SecurityKeyType.SymmetricKey) && (value != SecurityKeyType.AsymmetricKey))
            {
                return (value == SecurityKeyType.BearerKey);
            }
            return true;
        }

        internal static void Validate(SecurityKeyType value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(SecurityKeyType)));
            }
        }
    }
}

