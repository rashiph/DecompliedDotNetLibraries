namespace System.Transactions.Configuration
{
    using System;
    using System.Globalization;

    internal static class ConfigurationStrings
    {
        internal const string DefaultDistributedTransactionManagerName = "";
        internal const string DefaultMaxTimeout = "00:10:00";
        internal const string DefaultSettingsSectionName = "defaultSettings";
        internal const string DefaultTimeout = "00:01:00";
        internal const string DistributedTransactionManagerName = "distributedTransactionManagerName";
        internal const string MachineSettingsSectionName = "machineSettings";
        internal const string MaxTimeout = "maxTimeout";
        internal const string SectionGroupName = "system.transactions";
        internal const string Timeout = "timeout";
        internal const string TimeSpanZero = "00:00:00";

        internal static string GetSectionPath(string sectionName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[] { "system.transactions", sectionName });
        }

        internal static bool IsValidTimeSpan(TimeSpan span)
        {
            return (span >= TimeSpan.Zero);
        }

        internal static string DefaultSettingsSectionPath
        {
            get
            {
                return GetSectionPath("defaultSettings");
            }
        }

        internal static string MachineSettingsSectionPath
        {
            get
            {
                return GetSectionPath("machineSettings");
            }
        }
    }
}

