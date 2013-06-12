namespace System.Data.Odbc
{
    using System;

    internal sealed class DbSchemaInfo
    {
        internal int _columnlength;
        internal ODBC32.SQL_TYPE? _dbtype;
        internal int _lengthOffset;
        internal string _name;
        internal object _precision;
        internal object _scale;
        internal ODBC32.SQL_TYPE _sql_type;
        internal ODBC32.SQL_C _sqlctype;
        internal Type _type;
        internal string _typename;
        internal int _valueOffset;

        internal DbSchemaInfo()
        {
        }
    }
}

