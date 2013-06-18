namespace System.Data.SqlClient
{
    using System;

    internal sealed class SqlEnvChange
    {
        internal int length;
        internal byte[] newBinValue;
        internal SqlCollation newCollation;
        internal int newLength;
        internal long newLongValue;
        internal RoutingInfo newRoutingInfo;
        internal string newValue;
        internal byte[] oldBinValue;
        internal SqlCollation oldCollation;
        internal byte oldLength;
        internal long oldLongValue;
        internal string oldValue;
        internal byte type;
    }
}

