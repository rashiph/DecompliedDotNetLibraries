namespace System.IdentityModel.Selectors
{
    using System;
    using System.ComponentModel;
    using System.IdentityModel;

    public static class AudienceUriModeValidationHelper
    {
        public static bool IsDefined(AudienceUriMode validationMode)
        {
            if ((validationMode != AudienceUriMode.Never) && (validationMode != AudienceUriMode.Always))
            {
                return (validationMode == AudienceUriMode.BearerKeyOnly);
            }
            return true;
        }

        internal static void Validate(AudienceUriMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(AudienceUriMode)));
            }
        }
    }
}

