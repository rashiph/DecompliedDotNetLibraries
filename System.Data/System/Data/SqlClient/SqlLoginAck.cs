namespace System.Data.SqlClient
{
    using System;

    internal sealed class SqlLoginAck
    {
        internal short buildNum;
        internal bool isVersion8;
        internal byte majorVersion;
        internal byte minorVersion;
        internal string programName;
    }
}

