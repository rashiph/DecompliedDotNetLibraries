namespace System.Data.SqlClient
{
    using System;

    internal sealed class SqlLogin
    {
        internal string applicationName = "";
        internal string attachDBFilename = "";
        internal string database = "";
        internal string hostName = "";
        internal string language = "";
        internal string newPassword = "";
        internal int packetSize = 0x1f40;
        internal string password = "";
        internal bool readOnlyIntent;
        internal string serverName = "";
        internal int timeout;
        internal bool useReplication;
        internal bool userInstance;
        internal string userName = "";
        internal bool useSSPI;
    }
}

