namespace System.Data.SqlClient
{
    using System;

    internal sealed class SqlReturnValue : SqlMetaDataPriv
    {
        internal string parameter;
        internal ushort parmIndex;
        internal readonly SqlBuffer value = new SqlBuffer();

        internal SqlReturnValue()
        {
        }
    }
}

