namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Data;
    using System.Data.Common;
    using System.Text;
    using System.Web.UI.WebControls;

    internal sealed class SqlDataSourceTableQuery
    {
        private bool _asteriskField;
        private System.ComponentModel.Design.Data.DesignerDataConnection _designerDataConnection;
        private DesignerDataTableBase _designerDataTable;
        private bool _distinct;
        private IList<DesignerDataColumn> _fields = new List<DesignerDataColumn>();
        private IList<SqlDataSourceFilterClause> _filterClauses = new List<SqlDataSourceFilterClause>();
        private IList<SqlDataSourceOrderClause> _orderClauses = new List<SqlDataSourceOrderClause>();

        public SqlDataSourceTableQuery(System.ComponentModel.Design.Data.DesignerDataConnection designerDataConnection, DesignerDataTableBase designerDataTable)
        {
            this._designerDataConnection = designerDataConnection;
            this._designerDataTable = designerDataTable;
        }

        private void AppendWhereClauseParameter(StringBuilder commandText, SqlDataSourceColumnData columnData, string oldValuesFormatString)
        {
            string escapedName = columnData.EscapedName;
            string oldValueParameterPlaceHolder = columnData.GetOldValueParameterPlaceHolder(oldValuesFormatString);
            if (columnData.Column.Nullable)
            {
                commandText.Append("((");
                commandText.Append(escapedName);
                commandText.Append(" = ");
                commandText.Append(oldValueParameterPlaceHolder);
                commandText.Append(") OR (");
                commandText.Append(escapedName);
                commandText.Append(" IS NULL AND ");
                commandText.Append(oldValueParameterPlaceHolder);
                commandText.Append(" IS NULL))");
            }
            else
            {
                commandText.Append(escapedName);
                commandText.Append(" = ");
                commandText.Append(oldValueParameterPlaceHolder);
            }
        }

        private bool CanAutoGenerateQueries()
        {
            if (this.Distinct)
            {
                return false;
            }
            if (!this.AsteriskField && (this._fields.Count == 0))
            {
                return false;
            }
            return true;
        }

        public SqlDataSourceTableQuery Clone()
        {
            SqlDataSourceTableQuery query = new SqlDataSourceTableQuery(this.DesignerDataConnection, this.DesignerDataTable) {
                Distinct = this.Distinct,
                AsteriskField = this.AsteriskField
            };
            foreach (DesignerDataColumn column in this.Fields)
            {
                query.Fields.Add(column);
            }
            foreach (SqlDataSourceFilterClause clause in this.FilterClauses)
            {
                query.FilterClauses.Add(clause);
            }
            foreach (SqlDataSourceOrderClause clause2 in this.OrderClauses)
            {
                query.OrderClauses.Add(clause2);
            }
            return query;
        }

        public SqlDataSourceQuery GetDeleteQuery(string oldValuesFormatString, bool includeOldValues)
        {
            if (!this.CanAutoGenerateQueries())
            {
                return null;
            }
            StringBuilder builder = new StringBuilder("DELETE FROM ");
            builder.Append(this.GetTableName());
            SqlDataSourceQuery whereClause = this.GetWhereClause(oldValuesFormatString, includeOldValues);
            if (whereClause == null)
            {
                return null;
            }
            builder.Append(whereClause.Command);
            return new SqlDataSourceQuery(builder.ToString(), SqlDataSourceCommandType.Text, whereClause.Parameters);
        }

        private List<SqlDataSourceColumnData> GetEffectiveColumns()
        {
            StringCollection usedNames = new StringCollection();
            List<SqlDataSourceColumnData> list = new List<SqlDataSourceColumnData>();
            if (this.AsteriskField)
            {
                foreach (DesignerDataColumn column in this._designerDataTable.Columns)
                {
                    list.Add(new SqlDataSourceColumnData(this.DesignerDataConnection, column, usedNames));
                }
                return list;
            }
            foreach (DesignerDataColumn column2 in this._fields)
            {
                list.Add(new SqlDataSourceColumnData(this.DesignerDataConnection, column2, usedNames));
            }
            return list;
        }

        public SqlDataSourceQuery GetInsertQuery()
        {
            if (!this.CanAutoGenerateQueries())
            {
                return null;
            }
            List<Parameter> parameters = new List<Parameter>();
            StringBuilder builder = new StringBuilder("INSERT INTO ");
            builder.Append(this.GetTableName());
            List<SqlDataSourceColumnData> effectiveColumns = this.GetEffectiveColumns();
            StringBuilder builder2 = new StringBuilder();
            StringBuilder builder3 = new StringBuilder();
            bool flag = true;
            foreach (SqlDataSourceColumnData data in effectiveColumns)
            {
                if (!data.Column.Identity)
                {
                    if (!flag)
                    {
                        builder2.Append(", ");
                        builder3.Append(", ");
                    }
                    builder2.Append(data.EscapedName);
                    builder3.Append(data.ParameterPlaceholder);
                    DbProviderFactory dbProviderFactory = SqlDataSourceDesigner.GetDbProviderFactory(this.DesignerDataConnection.ProviderName);
                    parameters.Add(SqlDataSourceDesigner.CreateParameter(dbProviderFactory, data.WebParameterName, data.Column.DataType));
                    flag = false;
                }
            }
            if (flag)
            {
                return null;
            }
            builder.Append(" (");
            builder.Append(builder2.ToString());
            builder.Append(") VALUES (");
            builder.Append(builder3.ToString());
            builder.Append(")");
            return new SqlDataSourceQuery(builder.ToString(), SqlDataSourceCommandType.Text, parameters);
        }

        public SqlDataSourceQuery GetSelectQuery()
        {
            if (!this._asteriskField && (this._fields.Count == 0))
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(0x800);
            builder.Append("SELECT");
            if (this._distinct)
            {
                builder.Append(" DISTINCT");
            }
            if (this._asteriskField)
            {
                builder.Append(" ");
                SqlDataSourceColumnData data = new SqlDataSourceColumnData(this.DesignerDataConnection, null);
                builder.Append(data.SelectName);
            }
            if (this._fields.Count > 0)
            {
                builder.Append(" ");
                bool flag = true;
                foreach (SqlDataSourceColumnData data2 in this.GetEffectiveColumns())
                {
                    if (!flag)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(data2.SelectName);
                    flag = false;
                }
            }
            builder.Append(" FROM");
            builder.Append(" " + this.GetTableName());
            List<Parameter> list2 = new List<Parameter>();
            if (this._filterClauses.Count > 0)
            {
                builder.Append(" WHERE ");
                if (this._filterClauses.Count > 1)
                {
                    builder.Append("(");
                }
                bool flag2 = true;
                foreach (SqlDataSourceFilterClause clause in this._filterClauses)
                {
                    if (!flag2)
                    {
                        builder.Append(" AND ");
                    }
                    builder.Append("(" + clause.ToString() + ")");
                    flag2 = false;
                    if (clause.Parameter != null)
                    {
                        list2.Add(clause.Parameter);
                    }
                }
                if (this._filterClauses.Count > 1)
                {
                    builder.Append(")");
                }
            }
            if (this._orderClauses.Count > 0)
            {
                builder.Append(" ORDER BY ");
                bool flag3 = true;
                foreach (SqlDataSourceOrderClause clause2 in this._orderClauses)
                {
                    if (!flag3)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(clause2.ToString());
                    flag3 = false;
                }
            }
            return new SqlDataSourceQuery(builder.ToString(), SqlDataSourceCommandType.Text, list2.ToArray());
        }

        public string GetTableName()
        {
            return SqlDataSourceColumnData.EscapeObjectName(this.DesignerDataConnection, this.DesignerDataTable.Name);
        }

        public SqlDataSourceQuery GetUpdateQuery(string oldValuesFormatString, bool includeOldValues)
        {
            if (!this.CanAutoGenerateQueries())
            {
                return null;
            }
            StringBuilder builder = new StringBuilder("UPDATE ");
            builder.Append(this.GetTableName());
            builder.Append(" SET ");
            List<SqlDataSourceColumnData> effectiveColumns = this.GetEffectiveColumns();
            List<Parameter> parameters = new List<Parameter>();
            bool flag = true;
            foreach (SqlDataSourceColumnData data in effectiveColumns)
            {
                if (!data.Column.PrimaryKey)
                {
                    if (!flag)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(data.EscapedName);
                    builder.Append(" = ");
                    builder.Append(data.ParameterPlaceholder);
                    flag = false;
                    DbProviderFactory dbProviderFactory = SqlDataSourceDesigner.GetDbProviderFactory(this.DesignerDataConnection.ProviderName);
                    parameters.Add(SqlDataSourceDesigner.CreateParameter(dbProviderFactory, data.WebParameterName, data.Column.DataType));
                }
            }
            if (flag)
            {
                return null;
            }
            SqlDataSourceQuery whereClause = this.GetWhereClause(oldValuesFormatString, includeOldValues);
            if (whereClause == null)
            {
                return null;
            }
            builder.Append(whereClause.Command);
            foreach (Parameter parameter in whereClause.Parameters)
            {
                parameters.Add(parameter);
            }
            return new SqlDataSourceQuery(builder.ToString(), SqlDataSourceCommandType.Text, parameters);
        }

        private SqlDataSourceQuery GetWhereClause(string oldValuesFormatString, bool includeOldValues)
        {
            List<SqlDataSourceColumnData> effectiveColumns = this.GetEffectiveColumns();
            List<Parameter> parameters = new List<Parameter>();
            if (effectiveColumns.Count == 0)
            {
                return null;
            }
            StringBuilder commandText = new StringBuilder();
            commandText.Append(" WHERE ");
            int num = 0;
            DbProviderFactory dbProviderFactory = SqlDataSourceDesigner.GetDbProviderFactory(this.DesignerDataConnection.ProviderName);
            foreach (SqlDataSourceColumnData data in effectiveColumns)
            {
                if (data.Column.PrimaryKey)
                {
                    if (num > 0)
                    {
                        commandText.Append(" AND ");
                    }
                    num++;
                    this.AppendWhereClauseParameter(commandText, data, oldValuesFormatString);
                    parameters.Add(SqlDataSourceDesigner.CreateParameter(dbProviderFactory, data.GetOldValueWebParameterName(oldValuesFormatString), data.Column.DataType));
                }
            }
            if (num == 0)
            {
                return null;
            }
            if (includeOldValues)
            {
                foreach (SqlDataSourceColumnData data2 in effectiveColumns)
                {
                    if (!data2.Column.PrimaryKey)
                    {
                        commandText.Append(" AND ");
                        num++;
                        this.AppendWhereClauseParameter(commandText, data2, oldValuesFormatString);
                        Parameter item = SqlDataSourceDesigner.CreateParameter(dbProviderFactory, data2.GetOldValueWebParameterName(oldValuesFormatString), data2.Column.DataType);
                        parameters.Add(item);
                        if (data2.Column.Nullable && !SqlDataSourceDesigner.SupportsNamedParameters(dbProviderFactory))
                        {
                            parameters.Add(item);
                        }
                    }
                }
            }
            return new SqlDataSourceQuery(commandText.ToString(), SqlDataSourceCommandType.Text, parameters);
        }

        public bool IsPrimaryKeySelected()
        {
            List<SqlDataSourceColumnData> effectiveColumns = this.GetEffectiveColumns();
            if (effectiveColumns.Count == 0)
            {
                return false;
            }
            int num = 0;
            foreach (DesignerDataColumn column in this._designerDataTable.Columns)
            {
                if (column.PrimaryKey)
                {
                    num++;
                }
            }
            if (num == 0)
            {
                return false;
            }
            int num2 = 0;
            foreach (SqlDataSourceColumnData data in effectiveColumns)
            {
                if (data.Column.PrimaryKey)
                {
                    num2++;
                }
            }
            return (num == num2);
        }

        public bool AsteriskField
        {
            get
            {
                return this._asteriskField;
            }
            set
            {
                this._asteriskField = value;
                if (value)
                {
                    this.Fields.Clear();
                }
            }
        }

        public System.ComponentModel.Design.Data.DesignerDataConnection DesignerDataConnection
        {
            get
            {
                return this._designerDataConnection;
            }
        }

        public DesignerDataTableBase DesignerDataTable
        {
            get
            {
                return this._designerDataTable;
            }
        }

        public bool Distinct
        {
            get
            {
                return this._distinct;
            }
            set
            {
                this._distinct = value;
            }
        }

        public IList<DesignerDataColumn> Fields
        {
            get
            {
                return this._fields;
            }
        }

        public IList<SqlDataSourceFilterClause> FilterClauses
        {
            get
            {
                return this._filterClauses;
            }
        }

        public IList<SqlDataSourceOrderClause> OrderClauses
        {
            get
            {
                return this._orderClauses;
            }
        }
    }
}

