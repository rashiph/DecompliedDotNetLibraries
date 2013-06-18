namespace System.ServiceModel.Activation.Configuration
{
    using System;
    using System.Globalization;

    internal static class ConfigurationStrings
    {
        internal const string AllowAccounts = "allowAccounts";
        internal const string DiagnosticSectionName = "diagnostics";
        internal const string Enabled = "enabled";
        internal const string IIS_IUSRSSid = "S-1-5-32-568";
        internal const string ListenBacklog = "listenBacklog";
        internal const string MaxPendingAccepts = "maxPendingAccepts";
        internal const string MaxPendingConnections = "maxPendingConnections";
        internal const string NetPipeSectionName = "net.pipe";
        internal const string NetTcpSectionName = "net.tcp";
        internal const string PerformanceCountersEnabled = "performanceCountersEnabled";
        internal const string ReceiveTimeout = "receiveTimeout";
        internal const string SectionGroupName = "system.serviceModel.activation";
        internal const string SecurityIdentifier = "securityIdentifier";
        internal const string TeredoEnabled = "teredoEnabled";
        internal const string TimeSpanOneTick = "00:00:00.0000001";
        internal const string TimeSpanZero = "00:00:00";

        private static string GetSectionPath(string sectionName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[] { "system.serviceModel.activation", sectionName });
        }

        internal static string DiagnosticSectionPath
        {
            get
            {
                return GetSectionPath("diagnostics");
            }
        }

        internal static string NetPipeSectionPath
        {
            get
            {
                return GetSectionPath("net.pipe");
            }
        }

        internal static string NetTcpSectionPath
        {
            get
            {
                return GetSectionPath("net.tcp");
            }
        }
    }
}

