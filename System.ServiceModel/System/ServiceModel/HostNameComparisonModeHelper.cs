namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;

    internal static class HostNameComparisonModeHelper
    {
        internal static bool IsDefined(HostNameComparisonMode value)
        {
            if ((value != HostNameComparisonMode.StrongWildcard) && (value != HostNameComparisonMode.Exact))
            {
                return (value == HostNameComparisonMode.WeakWildcard);
            }
            return true;
        }

        public static void Validate(HostNameComparisonMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(HostNameComparisonMode)));
            }
        }
    }
}

