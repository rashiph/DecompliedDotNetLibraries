namespace System.Data.Sql
{
    using System;
    using System.Data;
    using System.Data.Common;

    internal sealed class SqlGenericUtil
    {
        private SqlGenericUtil()
        {
        }

        internal static Exception MismatchedMetaDataDirectionArrayLengths()
        {
            return ADP.Argument(Res.GetString("Sql_MismatchedMetaDataDirectionArrayLengths"));
        }

        internal static Exception NullCommandText()
        {
            return ADP.Argument(Res.GetString("Sql_NullCommandText"));
        }
    }
}

