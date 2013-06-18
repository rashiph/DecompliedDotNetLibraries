namespace System.Data.OracleClient
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal sealed class DbSqlParserTableCollection : CollectionBase
    {
        internal DbSqlParserTable Add(DbSqlParserTable value)
        {
            this.OnValidate(value);
            base.InnerList.Add(value);
            return value;
        }

        internal DbSqlParserTable Add(string databaseName, string schemaName, string tableName, string correlationName)
        {
            DbSqlParserTable table = new DbSqlParserTable(databaseName, schemaName, tableName, correlationName);
            return this.Add(table);
        }

        protected override void OnValidate(object value)
        {
        }

        internal DbSqlParserTable this[int i]
        {
            get
            {
                return (DbSqlParserTable) base.InnerList[i];
            }
        }

        private Type ItemType
        {
            get
            {
                return typeof(DbSqlParserTable);
            }
        }
    }
}

