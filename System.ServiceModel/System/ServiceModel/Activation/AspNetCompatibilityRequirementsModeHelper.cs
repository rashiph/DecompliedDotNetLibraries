namespace System.ServiceModel.Activation
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;

    internal static class AspNetCompatibilityRequirementsModeHelper
    {
        public static bool IsDefined(AspNetCompatibilityRequirementsMode x)
        {
            if ((x != AspNetCompatibilityRequirementsMode.NotAllowed) && (x != AspNetCompatibilityRequirementsMode.Allowed))
            {
                return (x == AspNetCompatibilityRequirementsMode.Required);
            }
            return true;
        }

        public static void Validate(AspNetCompatibilityRequirementsMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(AspNetCompatibilityRequirementsMode)));
            }
        }
    }
}

