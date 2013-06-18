namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;

    internal static class PerformanceCounterScopeHelper
    {
        internal static bool IsDefined(PerformanceCounterScope value)
        {
            if (((value != PerformanceCounterScope.Off) && (value != PerformanceCounterScope.Default)) && (value != PerformanceCounterScope.ServiceOnly))
            {
                return (value == PerformanceCounterScope.All);
            }
            return true;
        }

        public static void Validate(PerformanceCounterScope value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(PerformanceCounterScope)));
            }
        }
    }
}

