namespace System.Web.Configuration
{
    using System;
    using System.Collections;

    internal class RuleInfoComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            Type realType = ((HealthMonitoringSectionHelper.RuleInfo) x)._eventMappingSettings.RealType;
            Type o = ((HealthMonitoringSectionHelper.RuleInfo) y)._eventMappingSettings.RealType;
            if (realType.Equals(o))
            {
                return 0;
            }
            if (realType.IsSubclassOf(o))
            {
                return 1;
            }
            if (o.IsSubclassOf(realType))
            {
                return -1;
            }
            return string.Compare(realType.ToString(), o.ToString(), StringComparison.Ordinal);
        }
    }
}

