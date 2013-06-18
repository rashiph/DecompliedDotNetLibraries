namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Activities.Tracking;

    internal static class ImplementationVisibilityHelper
    {
        public static bool IsDefined(ImplementationVisibility value)
        {
            if (value != ImplementationVisibility.All)
            {
                return (value == ImplementationVisibility.RootScope);
            }
            return true;
        }
    }
}

