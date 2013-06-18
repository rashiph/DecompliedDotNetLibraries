namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;

    internal static class TokenReferenceStyleHelper
    {
        public static bool IsDefined(SecurityTokenReferenceStyle value)
        {
            if (value != SecurityTokenReferenceStyle.External)
            {
                return (value == SecurityTokenReferenceStyle.Internal);
            }
            return true;
        }

        public static void Validate(SecurityTokenReferenceStyle value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(SecurityTokenReferenceStyle)));
            }
        }
    }
}

