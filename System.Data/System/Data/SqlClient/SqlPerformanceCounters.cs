namespace System.Data.SqlClient
{
    using System;
    using System.Data.ProviderBase;
    using System.Diagnostics;
    using System.Security.Permissions;

    internal sealed class SqlPerformanceCounters : DbConnectionPoolCounters
    {
        private const string CategoryHelp = "Counters for System.Data.SqlClient";
        private const string CategoryName = ".NET Data Provider for SqlServer";
        public static readonly SqlPerformanceCounters SingletonInstance = new SqlPerformanceCounters();

        [PerformanceCounterPermission(SecurityAction.Assert, PermissionAccess=PerformanceCounterPermissionAccess.Write, MachineName=".", CategoryName=".NET Data Provider for SqlServer")]
        private SqlPerformanceCounters() : base(".NET Data Provider for SqlServer", "Counters for System.Data.SqlClient")
        {
        }
    }
}

