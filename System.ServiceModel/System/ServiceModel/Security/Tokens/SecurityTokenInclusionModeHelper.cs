namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;

    internal static class SecurityTokenInclusionModeHelper
    {
        public static bool IsDefined(SecurityTokenInclusionMode value)
        {
            if (((value != SecurityTokenInclusionMode.AlwaysToInitiator) && (value != SecurityTokenInclusionMode.AlwaysToRecipient)) && (value != SecurityTokenInclusionMode.Never))
            {
                return (value == SecurityTokenInclusionMode.Once);
            }
            return true;
        }

        public static void Validate(SecurityTokenInclusionMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(SecurityTokenInclusionMode)));
            }
        }
    }
}

