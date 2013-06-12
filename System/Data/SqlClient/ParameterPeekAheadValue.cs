namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;
    using System.Collections.Generic;

    internal class ParameterPeekAheadValue
    {
        internal IEnumerator<SqlDataRecord> Enumerator;
        internal SqlDataRecord FirstRecord;
    }
}

