namespace System.Data.OracleClient
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Text;

    internal sealed class OracleSqlParser : DbSqlParser
    {
        private OracleConnection _connection;
        private static readonly string _quoteCharacter = "\"";
        private static readonly string _regexPattern = DbSqlParser.CreateRegexPattern(@"[\p{Lo}\p{Lu}\p{Ll}\p{Lm}＿_#$]", @"[\p{Lo}\p{Lu}\p{Ll}\p{Lm}\p{Nd}＿_#$]", _quoteCharacter, "([^\"]|\"\")*", _quoteCharacter, "('([^']|'')*')");
        private static readonly string ConstraintOwnerParameterName = "OwnerName";
        private static readonly string ConstraintQuery1a = ("select ac.constraint_name key_name, acc.column_name key_col," + 1.ToString(CultureInfo.InvariantCulture) + " from all_cons_columns acc, all_constraints ac where acc.owner = ac.owner and acc.constraint_name = ac.constraint_name and acc.table_name = ac.table_name and ac.constraint_type = 'P'");
        private static readonly string ConstraintQuery1b_ownerDefault = " and ac.owner = user";
        private static readonly string ConstraintQuery1b_ownerIsKnown = " and ac.owner = :OwnerName";
        private static readonly string ConstraintQuery1c = " and ac.table_name = :TableName order by acc.constraint_name";
        private static readonly string ConstraintQuery2a = ("select aic.index_name key_name, aic.column_name key_col," + 3.ToString(CultureInfo.InvariantCulture) + " from all_ind_columns aic, all_indexes ai where aic.table_owner = ai.table_owner and aic.table_name = ai.table_name and aic.index_name = ai.index_name and ai.uniqueness = 'UNIQUE'");
        private static readonly string ConstraintQuery2b_ownerDefault = " and ai.owner = user";
        private static readonly string ConstraintQuery2b_ownerIsKnown = " and ai.owner = :OwnerName";
        private static readonly string ConstraintQuery2c = " and ai.table_name = :TableName order by aic.index_name";
        private static readonly string ConstraintTableParameterName = "TableName";
        private const string SynonymQueryBegin = "select table_owner, table_name from all_synonyms where";
        private const string SynonymQueryEnd = "' order by decode(owner, 'PUBLIC', 2, 1)";
        private const string SynonymQueryNoSchema = " owner in ('PUBLIC', user)";
        private const string SynonymQuerySchema = " owner = '";
        private const string SynonymQueryTable = " and synonym_name = '";

        internal OracleSqlParser() : base(_quoteCharacter, _quoteCharacter, _regexPattern)
        {
        }

        internal static string CatalogCase(string value)
        {
            if (System.Data.Common.ADP.IsEmpty(value))
            {
                return string.Empty;
            }
            if ('"' == value[0])
            {
                return value.Substring(1, value.Length - 2);
            }
            return value.ToUpper(CultureInfo.CurrentCulture);
        }

        protected override bool CatalogMatch(string valueA, string valueB)
        {
            if (System.Data.Common.ADP.IsEmpty(valueA) && System.Data.Common.ADP.IsEmpty(valueB))
            {
                return true;
            }
            if (System.Data.Common.ADP.IsEmpty(valueA) || System.Data.Common.ADP.IsEmpty(valueB))
            {
                return false;
            }
            bool flag2 = '"' == valueA[0];
            int num4 = 0;
            int length = valueA.Length;
            bool flag = '"' == valueB[0];
            int num2 = 0;
            int num = valueB.Length;
            if (flag2)
            {
                num4++;
                length -= 2;
            }
            if (flag)
            {
                num2++;
                num -= 2;
            }
            CompareOptions options = CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType;
            if (!flag2 || !flag)
            {
                options |= CompareOptions.IgnoreCase;
            }
            int num5 = CultureInfo.CurrentCulture.CompareInfo.Compare(valueA, num4, length, valueB, num2, num, options);
            return (0 == num5);
        }

        private DbSqlParserColumn FindConstraintColumn(string schemaName, string tableName, string columnName)
        {
            DbSqlParserColumnCollection columns = base.Columns;
            int count = columns.Count;
            for (int i = 0; i < count; i++)
            {
                DbSqlParserColumn column = columns[i];
                if ((this.CatalogMatch(column.SchemaName, schemaName) && this.CatalogMatch(column.TableName, tableName)) && this.CatalogMatch(column.ColumnName, columnName))
                {
                    return column;
                }
            }
            return null;
        }

        protected override void GatherKeyColumns(DbSqlParserTable table)
        {
            using (OracleCommand command = this._connection.CreateCommand())
            {
                command.Transaction = this._connection.Transaction;
                string schemaName = CatalogCase(table.SchemaName);
                string tableName = CatalogCase(table.TableName);
                string str = schemaName;
                string str4 = tableName;
                command.CommandText = this.GetSynonymQueryStatement(schemaName, tableName);
                using (OracleDataReader reader2 = command.ExecuteReader())
                {
                    if (reader2.Read())
                    {
                        str = reader2.GetString(0);
                        str4 = reader2.GetString(1);
                    }
                }
                StringBuilder builder2 = new StringBuilder(ConstraintQuery1a);
                StringBuilder builder = new StringBuilder(ConstraintQuery2a);
                if (System.Data.Common.ADP.IsEmpty(str))
                {
                    builder2.Append(ConstraintQuery1b_ownerDefault);
                    builder.Append(ConstraintQuery2b_ownerDefault);
                }
                else
                {
                    command.Parameters.Add(new OracleParameter(ConstraintOwnerParameterName, DbType.String)).Value = str;
                    builder2.Append(ConstraintQuery1b_ownerIsKnown);
                    builder.Append(ConstraintQuery2b_ownerIsKnown);
                }
                command.Parameters.Add(new OracleParameter(ConstraintTableParameterName, DbType.String)).Value = str4;
                builder2.Append(ConstraintQuery1c);
                builder.Append(ConstraintQuery2c);
                string[] strArray3 = new string[] { builder2.ToString(), builder.ToString() };
                foreach (string str6 in strArray3)
                {
                    command.CommandText = str6;
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        ArrayList list = new ArrayList();
                        bool flag2 = reader.Read();
                        bool flag = false;
                        while (flag2)
                        {
                            ConstraintColumn column;
                            list.Clear();
                            string str5 = reader.GetString(0);
                            do
                            {
                                column = new ConstraintColumn {
                                    columnName = reader.GetString(1),
                                    constraintType = (DbSqlParserColumn.ConstraintType) ((int) reader.GetDecimal(2)),
                                    parsedColumn = null
                                };
                                list.Add(column);
                                flag2 = reader.Read();
                            }
                            while (flag2 && (str5 == reader.GetString(0)));
                            flag = true;
                            for (int i = 0; i < list.Count; i++)
                            {
                                column = (ConstraintColumn) list[i];
                                column.parsedColumn = this.FindConstraintColumn(schemaName, tableName, column.columnName);
                                if (column.parsedColumn == null)
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                for (int j = 0; j < list.Count; j++)
                                {
                                    column = (ConstraintColumn) list[j];
                                    column.parsedColumn.SetConstraint(column.constraintType);
                                }
                                break;
                            }
                        }
                        if (flag)
                        {
                            return;
                        }
                    }
                }
            }
        }

        protected override DbSqlParserColumnCollection GatherTableColumns(DbSqlParserTable table)
        {
            OciStatementHandle stmtp = new OciStatementHandle(this._connection.ServiceContextHandle);
            OciErrorHandle errorHandle = this._connection.ErrorHandle;
            StringBuilder builder = new StringBuilder();
            string schemaName = table.SchemaName;
            string tableName = table.TableName;
            DbSqlParserColumnCollection columns = new DbSqlParserColumnCollection();
            builder.Append("select * from ");
            if (!System.Data.Common.ADP.IsEmpty(schemaName))
            {
                builder.Append(schemaName);
                builder.Append(".");
            }
            builder.Append(tableName);
            string stmt = builder.ToString();
            if ((TracedNativeMethods.OCIStmtPrepare(stmtp, errorHandle, stmt, OCI.SYNTAX.OCI_NTV_SYNTAX, OCI.MODE.OCI_DEFAULT, this._connection) == 0) && (TracedNativeMethods.OCIStmtExecute(this._connection.ServiceContextHandle, stmtp, errorHandle, 0, OCI.MODE.OCI_DESCRIBE_ONLY) == 0))
            {
                int num3;
                stmtp.GetAttribute(OCI.ATTR.OCI_ATTR_PARAM_COUNT, out num3, errorHandle);
                for (int i = 0; i < num3; i++)
                {
                    string str;
                    OciParameterDescriptor handle = stmtp.GetDescriptor(i, errorHandle);
                    handle.GetAttribute(OCI.ATTR.OCI_ATTR_SQLCODE, out str, errorHandle, this._connection);
                    OciHandle.SafeDispose(ref handle);
                    str = this.QuotePrefixCharacter + str + this.QuoteSuffixCharacter;
                    columns.Add(null, schemaName, tableName, str, null);
                }
            }
            OciHandle.SafeDispose(ref stmtp);
            return columns;
        }

        private string GetSynonymQueryStatement(string schemaName, string tableName)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("select table_owner, table_name from all_synonyms where");
            if (System.Data.Common.ADP.IsEmpty(schemaName))
            {
                builder.Append(" owner in ('PUBLIC', user)");
            }
            else
            {
                builder.Append(" owner = '");
                builder.Append(schemaName);
                builder.Append("'");
            }
            builder.Append(" and synonym_name = '");
            builder.Append(tableName);
            builder.Append("' order by decode(owner, 'PUBLIC', 2, 1)");
            return builder.ToString();
        }

        internal void Parse(string statementText, OracleConnection connection)
        {
            this._connection = connection;
            base.Parse(statementText);
        }

        private sealed class ConstraintColumn
        {
            internal string columnName;
            internal DbSqlParserColumn.ConstraintType constraintType;
            internal DbSqlParserColumn parsedColumn;
        }
    }
}

