namespace System.Data.SqlClient
{
    using System;

    internal sealed class _SqlRPC
    {
        internal int cumulativeRecordsAffected;
        internal string databaseName;
        internal SqlErrorCollection errors;
        internal int errorsIndexEnd;
        internal int errorsIndexStart;
        internal ushort options;
        internal SqlParameter[] parameters;
        internal byte[] paramoptions;
        internal ushort ProcID;
        internal int? recordsAffected;
        internal string rpcName;
        internal SqlErrorCollection warnings;
        internal int warningsIndexEnd;
        internal int warningsIndexStart;
    }
}

