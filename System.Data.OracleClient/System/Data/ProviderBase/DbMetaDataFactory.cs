namespace System.Data.ProviderBase
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.IO;

    internal class DbMetaDataFactory
    {
        private const string _collectionName = "CollectionName";
        private const string _dataSourceProductVersion = "DataSourceProductVersion";
        private const string _dataSourceProductVersionNormalized = "DataSourceProductVersionNormalized";
        private const string _dataTable = "DataTable";
        private const string _maximumVersion = "MaximumVersion";
        private DataSet _metaDataCollectionsDataSet;
        private const string _minimumVersion = "MinimumVersion";
        private string _normalizedServerVersion;
        private const string _numberOfRestrictions = "NumberOfRestrictions";
        private const string _parameterName = "ParameterName";
        private const string _populationMechanism = "PopulationMechanism";
        private const string _populationString = "PopulationString";
        private const string _prepareCollection = "PrepareCollection";
        private const string _restrictionDefault = "RestrictionDefault";
        private const string _restrictionName = "RestrictionName";
        private const string _restrictionNumber = "RestrictionNumber";
        private string _serverVersionString;
        private const string _sqlCommand = "SQLCommand";

        public DbMetaDataFactory(Stream xmlStream, string serverVersion, string normalizedServerVersion)
        {
            System.Data.Common.ADP.CheckArgumentNull(xmlStream, "xmlStream");
            System.Data.Common.ADP.CheckArgumentNull(serverVersion, "serverVersion");
            System.Data.Common.ADP.CheckArgumentNull(normalizedServerVersion, "normalizedServerVersion");
            this.LoadDataSetFromXml(xmlStream);
            this._serverVersionString = serverVersion;
            this._normalizedServerVersion = normalizedServerVersion;
        }

        protected DataTable CloneAndFilterCollection(string collectionName, string[] hiddenColumnNames)
        {
            DataTable sourceTable = this._metaDataCollectionsDataSet.Tables[collectionName];
            if ((sourceTable == null) || (collectionName != sourceTable.TableName))
            {
                throw System.Data.Common.ADP.DataTableDoesNotExist(collectionName);
            }
            DataTable table = new DataTable(collectionName) {
                Locale = CultureInfo.InvariantCulture
            };
            DataColumnCollection destinationColumns = table.Columns;
            DataColumn[] columnArray = this.FilterColumns(sourceTable, hiddenColumnNames, destinationColumns);
            foreach (DataRow row2 in sourceTable.Rows)
            {
                if (this.SupportedByCurrentVersion(row2))
                {
                    DataRow row = table.NewRow();
                    for (int i = 0; i < destinationColumns.Count; i++)
                    {
                        row[destinationColumns[i]] = row2[columnArray[i], DataRowVersion.Current];
                    }
                    table.Rows.Add(row);
                    row.AcceptChanges();
                }
            }
            return table;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._normalizedServerVersion = null;
                this._serverVersionString = null;
                this._metaDataCollectionsDataSet.Dispose();
            }
        }

        private DataTable ExecuteCommand(DataRow requestedCollectionRow, string[] restrictions, DbConnection connection)
        {
            DataTable table2 = this._metaDataCollectionsDataSet.Tables[DbMetaDataCollectionNames.MetaDataCollections];
            DataColumn column3 = table2.Columns["PopulationString"];
            DataColumn column2 = table2.Columns["NumberOfRestrictions"];
            DataColumn column = table2.Columns["CollectionName"];
            DataTable table = null;
            DbCommand command = null;
            string str2 = requestedCollectionRow[column3, DataRowVersion.Current] as string;
            int num2 = (int) requestedCollectionRow[column2, DataRowVersion.Current];
            string collectionName = requestedCollectionRow[column, DataRowVersion.Current] as string;
            if ((restrictions != null) && (restrictions.Length > num2))
            {
                throw System.Data.Common.ADP.TooManyRestrictions(collectionName);
            }
            command = connection.CreateCommand();
            command.CommandText = str2;
            command.CommandTimeout = Math.Max(command.CommandTimeout, 180);
            for (int i = 0; i < num2; i++)
            {
                DbParameter parameter = command.CreateParameter();
                if (((restrictions != null) && (restrictions.Length > i)) && (restrictions[i] != null))
                {
                    parameter.Value = restrictions[i];
                }
                else
                {
                    parameter.Value = DBNull.Value;
                }
                parameter.ParameterName = this.GetParameterName(collectionName, i + 1);
                parameter.Direction = ParameterDirection.Input;
                command.Parameters.Add(parameter);
            }
            DbDataReader reader = null;
            try
            {
                try
                {
                    reader = command.ExecuteReader();
                }
                catch (Exception exception)
                {
                    if (!System.Data.Common.ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    throw System.Data.Common.ADP.QueryFailed(collectionName, exception);
                }
                table = new DataTable(collectionName) {
                    Locale = CultureInfo.InvariantCulture
                };
                foreach (DataRow row in reader.GetSchemaTable().Rows)
                {
                    table.Columns.Add(row["ColumnName"] as string, (Type) row["DataType"]);
                }
                object[] values = new object[table.Columns.Count];
                while (reader.Read())
                {
                    reader.GetValues(values);
                    table.Rows.Add(values);
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }
            }
            return table;
        }

        private DataColumn[] FilterColumns(DataTable sourceTable, string[] hiddenColumnNames, DataColumnCollection destinationColumns)
        {
            DataColumn[] columnArray = null;
            int num = 0;
            foreach (DataColumn column3 in sourceTable.Columns)
            {
                if (this.IncludeThisColumn(column3, hiddenColumnNames))
                {
                    num++;
                }
            }
            if (num == 0)
            {
                throw System.Data.Common.ADP.NoColumns();
            }
            int index = 0;
            columnArray = new DataColumn[num];
            foreach (DataColumn column in sourceTable.Columns)
            {
                if (this.IncludeThisColumn(column, hiddenColumnNames))
                {
                    DataColumn column2 = new DataColumn(column.ColumnName, column.DataType);
                    destinationColumns.Add(column2);
                    columnArray[index] = column;
                    index++;
                }
            }
            return columnArray;
        }

        internal DataRow FindMetaDataCollectionRow(string collectionName)
        {
            DataTable table = this._metaDataCollectionsDataSet.Tables[DbMetaDataCollectionNames.MetaDataCollections];
            if (table == null)
            {
                throw System.Data.Common.ADP.InvalidXml();
            }
            DataColumn column = table.Columns[DbMetaDataColumnNames.CollectionName];
            if ((column == null) || (typeof(string) != column.DataType))
            {
                throw System.Data.Common.ADP.InvalidXmlMissingColumn(DbMetaDataCollectionNames.MetaDataCollections, DbMetaDataColumnNames.CollectionName);
            }
            DataRow row2 = null;
            string str2 = null;
            bool flag3 = false;
            bool flag = false;
            bool flag2 = false;
            foreach (DataRow row in table.Rows)
            {
                string str = row[column, DataRowVersion.Current] as string;
                if (System.Data.Common.ADP.IsEmpty(str))
                {
                    throw System.Data.Common.ADP.InvalidXmlInvalidValue(DbMetaDataCollectionNames.MetaDataCollections, DbMetaDataColumnNames.CollectionName);
                }
                if (System.Data.Common.ADP.CompareInsensitiveInvariant(str, collectionName))
                {
                    if (!this.SupportedByCurrentVersion(row))
                    {
                        flag3 = true;
                    }
                    else if (collectionName == str)
                    {
                        if (flag)
                        {
                            throw System.Data.Common.ADP.CollectionNameIsNotUnique(collectionName);
                        }
                        row2 = row;
                        str2 = str;
                        flag = true;
                    }
                    else
                    {
                        if (str2 != null)
                        {
                            flag2 = true;
                        }
                        row2 = row;
                        str2 = str;
                    }
                }
            }
            if (row2 == null)
            {
                if (!flag3)
                {
                    throw System.Data.Common.ADP.UndefinedCollection(collectionName);
                }
                throw System.Data.Common.ADP.UnsupportedVersion(collectionName);
            }
            if (!flag && flag2)
            {
                throw System.Data.Common.ADP.AmbigousCollectionName(collectionName);
            }
            return row2;
        }

        private void FixUpVersion(DataTable dataSourceInfoTable)
        {
            DataColumn column2 = dataSourceInfoTable.Columns["DataSourceProductVersion"];
            DataColumn column = dataSourceInfoTable.Columns["DataSourceProductVersionNormalized"];
            if ((column2 == null) || (column == null))
            {
                throw System.Data.Common.ADP.MissingDataSourceInformationColumn();
            }
            if (dataSourceInfoTable.Rows.Count != 1)
            {
                throw System.Data.Common.ADP.IncorrectNumberOfDataSourceInformationRows();
            }
            DataRow row = dataSourceInfoTable.Rows[0];
            row[column2] = this._serverVersionString;
            row[column] = this._normalizedServerVersion;
            row.AcceptChanges();
        }

        private string GetParameterName(string neededCollectionName, int neededRestrictionNumber)
        {
            DataColumnCollection columns = null;
            DataTable table = null;
            string str = null;
            DataColumn column = null;
            DataColumn column2 = null;
            DataColumn column3 = null;
            DataColumn column4 = null;
            table = this._metaDataCollectionsDataSet.Tables[DbMetaDataCollectionNames.Restrictions];
            if (table != null)
            {
                columns = table.Columns;
                if (columns != null)
                {
                    column3 = columns["CollectionName"];
                    column2 = columns["ParameterName"];
                    column4 = columns["RestrictionName"];
                    column = columns["RestrictionNumber"];
                }
            }
            if (((column2 == null) || (column3 == null)) || ((column4 == null) || (column == null)))
            {
                throw System.Data.Common.ADP.MissingRestrictionColumn();
            }
            foreach (DataRow row in table.Rows)
            {
                if (((((string) row[column3]) == neededCollectionName) && (((int) row[column]) == neededRestrictionNumber)) && this.SupportedByCurrentVersion(row))
                {
                    str = (string) row[column2];
                    break;
                }
            }
            if (str == null)
            {
                throw System.Data.Common.ADP.MissingRestrictionRow();
            }
            return str;
        }

        public virtual DataTable GetSchema(DbConnection connection, string collectionName, string[] restrictions)
        {
            DataTable table2 = this._metaDataCollectionsDataSet.Tables[DbMetaDataCollectionNames.MetaDataCollections];
            DataColumn column2 = table2.Columns["PopulationMechanism"];
            DataColumn column = table2.Columns[DbMetaDataColumnNames.CollectionName];
            DataRow requestedCollectionRow = null;
            DataTable dataSourceInfoTable = null;
            string str = null;
            requestedCollectionRow = this.FindMetaDataCollectionRow(collectionName);
            str = requestedCollectionRow[column, DataRowVersion.Current] as string;
            if (!System.Data.Common.ADP.IsEmptyArray(restrictions))
            {
                for (int i = 0; i < restrictions.Length; i++)
                {
                    if ((restrictions[i] != null) && (restrictions[i].Length > 0x1000))
                    {
                        throw System.Data.Common.ADP.NotSupported();
                    }
                }
            }
            string populationMechanism = requestedCollectionRow[column2, DataRowVersion.Current] as string;
            switch (populationMechanism)
            {
                case "DataTable":
                    string[] strArray;
                    if (str == DbMetaDataCollectionNames.MetaDataCollections)
                    {
                        strArray = new string[] { "PopulationMechanism", "PopulationString" };
                    }
                    else
                    {
                        strArray = null;
                    }
                    if (!System.Data.Common.ADP.IsEmptyArray(restrictions))
                    {
                        throw System.Data.Common.ADP.TooManyRestrictions(str);
                    }
                    dataSourceInfoTable = this.CloneAndFilterCollection(str, strArray);
                    if (str == DbMetaDataCollectionNames.DataSourceInformation)
                    {
                        this.FixUpVersion(dataSourceInfoTable);
                    }
                    return dataSourceInfoTable;

                case "SQLCommand":
                    return this.ExecuteCommand(requestedCollectionRow, restrictions, connection);

                case "PrepareCollection":
                    return this.PrepareCollection(str, restrictions, connection);
            }
            throw System.Data.Common.ADP.UndefinedPopulationMechanism(populationMechanism);
        }

        private bool IncludeThisColumn(DataColumn sourceColumn, string[] hiddenColumnNames)
        {
            string str;
            string columnName = sourceColumn.ColumnName;
            if (((str = columnName) != null) && ((str == "MinimumVersion") || (str == "MaximumVersion")))
            {
                return false;
            }
            if (hiddenColumnNames != null)
            {
                for (int i = 0; i < hiddenColumnNames.Length; i++)
                {
                    if (hiddenColumnNames[i] == columnName)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void LoadDataSetFromXml(Stream XmlStream)
        {
            this._metaDataCollectionsDataSet = new DataSet();
            this._metaDataCollectionsDataSet.Locale = CultureInfo.InvariantCulture;
            this._metaDataCollectionsDataSet.ReadXml(XmlStream);
        }

        protected virtual DataTable PrepareCollection(string collectionName, string[] restrictions, DbConnection connection)
        {
            throw System.Data.Common.ADP.NotSupported();
        }

        private bool SupportedByCurrentVersion(DataRow requestedCollectionRow)
        {
            object obj2;
            bool flag = true;
            DataColumnCollection columns = requestedCollectionRow.Table.Columns;
            DataColumn column = columns["MinimumVersion"];
            if (column != null)
            {
                obj2 = requestedCollectionRow[column];
                if (((obj2 != null) && (obj2 != DBNull.Value)) && (0 > string.Compare(this._normalizedServerVersion, (string) obj2, StringComparison.OrdinalIgnoreCase)))
                {
                    flag = false;
                }
            }
            if (flag)
            {
                column = columns["MaximumVersion"];
                if (column != null)
                {
                    obj2 = requestedCollectionRow[column];
                    if (((obj2 != null) && (obj2 != DBNull.Value)) && (0 < string.Compare(this._normalizedServerVersion, (string) obj2, StringComparison.OrdinalIgnoreCase)))
                    {
                        flag = false;
                    }
                }
            }
            return flag;
        }
    }
}

