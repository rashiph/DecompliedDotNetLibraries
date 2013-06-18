namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;

    internal static class SecurityHeaderLayoutHelper
    {
        public static bool IsDefined(SecurityHeaderLayout value)
        {
            if (((value != SecurityHeaderLayout.Lax) && (value != SecurityHeaderLayout.LaxTimestampFirst)) && (value != SecurityHeaderLayout.LaxTimestampLast))
            {
                return (value == SecurityHeaderLayout.Strict);
            }
            return true;
        }

        public static void Validate(SecurityHeaderLayout value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(SecurityHeaderLayout)));
            }
        }
    }
}

