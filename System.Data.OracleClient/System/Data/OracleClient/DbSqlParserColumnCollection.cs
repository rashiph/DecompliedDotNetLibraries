namespace System.Data.OracleClient
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal sealed class DbSqlParserColumnCollection : CollectionBase
    {
        internal DbSqlParserColumn Add(DbSqlParserColumn value)
        {
            this.OnValidate(value);
            base.InnerList.Add(value);
            return value;
        }

        internal DbSqlParserColumn Add(string databaseName, string schemaName, string tableName, string columnName, string alias)
        {
            DbSqlParserColumn column = new DbSqlParserColumn(databaseName, schemaName, tableName, columnName, alias);
            return this.Add(column);
        }

        internal void Insert(int index, DbSqlParserColumn value)
        {
            base.InnerList.Insert(index, value);
        }

        protected override void OnValidate(object value)
        {
        }

        internal DbSqlParserColumn this[int i]
        {
            get
            {
                return (DbSqlParserColumn) base.InnerList[i];
            }
        }

        private Type ItemType
        {
            get
            {
                return typeof(DbSqlParserColumn);
            }
        }
    }
}

