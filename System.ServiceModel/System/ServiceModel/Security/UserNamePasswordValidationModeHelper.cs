namespace System.ServiceModel.Security
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;

    internal static class UserNamePasswordValidationModeHelper
    {
        public static bool IsDefined(UserNamePasswordValidationMode validationMode)
        {
            if ((validationMode != UserNamePasswordValidationMode.Windows) && (validationMode != UserNamePasswordValidationMode.MembershipProvider))
            {
                return (validationMode == UserNamePasswordValidationMode.Custom);
            }
            return true;
        }

        public static void Validate(UserNamePasswordValidationMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(UserNamePasswordValidationMode)));
            }
        }
    }
}

