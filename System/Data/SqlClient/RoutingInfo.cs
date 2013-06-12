namespace System.Data.SqlClient
{
    using System;
    using System.Runtime.CompilerServices;

    internal class RoutingInfo
    {
        internal RoutingInfo(byte protocol, ushort port, string servername)
        {
            this.Protocol = protocol;
            this.Port = port;
            this.ServerName = servername;
        }

        internal ushort Port { get; private set; }

        internal byte Protocol { get; private set; }

        internal string ServerName { get; private set; }
    }
}

