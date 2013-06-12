namespace System.Data.SqlClient
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal sealed class ServerInfo
    {
        private string m_userServerName;
        internal readonly string PreRoutingServerName;

        internal ServerInfo(SqlConnectionString userOptions) : this(userOptions, userOptions.DataSource)
        {
        }

        internal ServerInfo(SqlConnectionString userOptions, string serverName)
        {
            this.UserServerName = serverName ?? string.Empty;
            this.UserProtocol = userOptions.NetworkLibrary;
            this.ResolvedDatabaseName = userOptions.InitialCatalog;
            this.PreRoutingServerName = null;
        }

        internal ServerInfo(SqlConnectionString userOptions, RoutingInfo routing, string preRoutingServerName)
        {
            if ((routing == null) || (routing.ServerName == null))
            {
                this.UserServerName = string.Empty;
            }
            else
            {
                this.UserServerName = string.Format(CultureInfo.InvariantCulture, "{0},{1}", new object[] { routing.ServerName, routing.Port });
            }
            this.PreRoutingServerName = preRoutingServerName;
            this.UserProtocol = "tcp";
            this.SetDerivedNames(this.UserProtocol, this.UserServerName);
            this.ResolvedDatabaseName = userOptions.InitialCatalog;
        }

        internal void SetDerivedNames(string protocol, string serverName)
        {
            if (!ADP.IsEmpty(protocol))
            {
                this.ExtendedServerName = protocol + ":" + serverName;
            }
            else
            {
                this.ExtendedServerName = serverName;
            }
            this.ResolvedServerName = serverName;
        }

        internal string ExtendedServerName { get; private set; }

        internal string ResolvedDatabaseName { get; private set; }

        internal string ResolvedServerName { get; private set; }

        internal string UserProtocol { get; private set; }

        internal string UserServerName
        {
            get
            {
                return this.m_userServerName;
            }
            private set
            {
                this.m_userServerName = value;
            }
        }
    }
}

