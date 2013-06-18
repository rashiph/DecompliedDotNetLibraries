namespace System.Data.OracleClient
{
    using System;
    using System.Data.ProviderBase;
    using System.Diagnostics;
    using System.Security.Permissions;

    internal sealed class OraclePerformanceCounters : System.Data.ProviderBase.DbConnectionPoolCounters
    {
        private const string CategoryHelp = "Counters for System.Data.OracleClient";
        private const string CategoryName = ".NET Data Provider for Oracle";
        public static readonly OraclePerformanceCounters SingletonInstance = new OraclePerformanceCounters();

        [PerformanceCounterPermission(SecurityAction.Assert, PermissionAccess=PerformanceCounterPermissionAccess.Write, MachineName=".", CategoryName=".NET Data Provider for Oracle")]
        private OraclePerformanceCounters() : base(".NET Data Provider for Oracle", "Counters for System.Data.OracleClient")
        {
        }
    }
}

