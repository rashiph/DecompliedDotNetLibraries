namespace System.Data.OleDb
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal sealed class OleDbMetaDataFactory : DbMetaDataFactory
    {
        private const string _collectionName = "CollectionName";
        private const string _populationMechanism = "PopulationMechanism";
        private const string _prepareCollection = "PrepareCollection";
        private readonly SchemaRowsetName[] _schemaMapping;

        internal OleDbMetaDataFactory(Stream XMLStream, string serverVersion, string serverVersionNormalized, SchemaSupport[] schemaSupport) : base(XMLStream, serverVersion, serverVersionNormalized)
        {
            this._schemaMapping = new SchemaRowsetName[] { new SchemaRowsetName(DbMetaDataCollectionNames.DataTypes, OleDbSchemaGuid.Provider_Types), new SchemaRowsetName(OleDbMetaDataCollectionNames.Catalogs, OleDbSchemaGuid.Catalogs), new SchemaRowsetName(OleDbMetaDataCollectionNames.Collations, OleDbSchemaGuid.Collations), new SchemaRowsetName(OleDbMetaDataCollectionNames.Columns, OleDbSchemaGuid.Columns), new SchemaRowsetName(OleDbMetaDataCollectionNames.Indexes, OleDbSchemaGuid.Indexes), new SchemaRowsetName(OleDbMetaDataCollectionNames.Procedures, OleDbSchemaGuid.Procedures), new SchemaRowsetName(OleDbMetaDataCollectionNames.ProcedureColumns, OleDbSchemaGuid.Procedure_Columns), new SchemaRowsetName(OleDbMetaDataCollectionNames.ProcedureParameters, OleDbSchemaGuid.Procedure_Parameters), new SchemaRowsetName(OleDbMetaDataCollectionNames.Tables, OleDbSchemaGuid.Tables), new SchemaRowsetName(OleDbMetaDataCollectionNames.Views, OleDbSchemaGuid.Views) };
            if (base.CollectionDataSet.Tables[DbMetaDataCollectionNames.MetaDataCollections] == null)
            {
                throw ADP.UnableToBuildCollection(DbMetaDataCollectionNames.MetaDataCollections);
            }
            DataTable table2 = base.CloneAndFilterCollection(DbMetaDataCollectionNames.MetaDataCollections, null);
            DataTable table = base.CollectionDataSet.Tables[DbMetaDataCollectionNames.Restrictions];
            if (table != null)
            {
                table = base.CloneAndFilterCollection(DbMetaDataCollectionNames.Restrictions, null);
            }
            DataColumn column3 = table2.Columns["PopulationMechanism"];
            if ((column3 == null) || (typeof(string) != column3.DataType))
            {
                throw ADP.InvalidXmlMissingColumn(DbMetaDataCollectionNames.MetaDataCollections, "PopulationMechanism");
            }
            DataColumn column2 = table2.Columns["CollectionName"];
            if ((column2 == null) || (typeof(string) != column2.DataType))
            {
                throw ADP.InvalidXmlMissingColumn(DbMetaDataCollectionNames.MetaDataCollections, "CollectionName");
            }
            DataColumn column = null;
            if (table != null)
            {
                column = table.Columns["CollectionName"];
                if ((column == null) || (typeof(string) != column.DataType))
                {
                    throw ADP.InvalidXmlMissingColumn(DbMetaDataCollectionNames.Restrictions, "CollectionName");
                }
            }
            foreach (DataRow row in table2.Rows)
            {
                string str3 = row[column3] as string;
                if (ADP.IsEmpty(str3))
                {
                    throw ADP.InvalidXmlInvalidValue(DbMetaDataCollectionNames.MetaDataCollections, "PopulationMechanism");
                }
                string str = row[column2] as string;
                if (ADP.IsEmpty(str))
                {
                    throw ADP.InvalidXmlInvalidValue(DbMetaDataCollectionNames.MetaDataCollections, "CollectionName");
                }
                if (str3 == "PrepareCollection")
                {
                    int index = -1;
                    for (int i = 0; i < this._schemaMapping.Length; i++)
                    {
                        if (this._schemaMapping[i]._schemaName == str)
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index != -1)
                    {
                        bool flag = false;
                        if (schemaSupport != null)
                        {
                            for (int j = 0; j < schemaSupport.Length; j++)
                            {
                                if (this._schemaMapping[index]._schemaRowset == schemaSupport[j]._schemaRowset)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                        {
                            if (table != null)
                            {
                                foreach (DataRow row2 in table.Rows)
                                {
                                    string str2 = row2[column] as string;
                                    if (ADP.IsEmpty(str2))
                                    {
                                        throw ADP.InvalidXmlInvalidValue(DbMetaDataCollectionNames.Restrictions, "CollectionName");
                                    }
                                    if (str == str2)
                                    {
                                        row2.Delete();
                                    }
                                }
                                table.AcceptChanges();
                            }
                            row.Delete();
                        }
                    }
                }
            }
            table2.AcceptChanges();
            base.CollectionDataSet.Tables.Remove(base.CollectionDataSet.Tables[DbMetaDataCollectionNames.MetaDataCollections]);
            base.CollectionDataSet.Tables.Add(table2);
            if (table != null)
            {
                base.CollectionDataSet.Tables.Remove(base.CollectionDataSet.Tables[DbMetaDataCollectionNames.Restrictions]);
                base.CollectionDataSet.Tables.Add(table);
            }
        }

        private string BuildRegularExpression(string invalidChars, string invalidStartingChars)
        {
            StringBuilder escapedString = new StringBuilder("[^");
            ADP.EscapeSpecialCharacters(invalidStartingChars, escapedString);
            escapedString.Append("][^");
            ADP.EscapeSpecialCharacters(invalidChars, escapedString);
            escapedString.Append("]*");
            return escapedString.ToString();
        }

        private DataTable GetDataSourceInformationTable(OleDbConnection connection, OleDbConnectionInternal internalConnection)
        {
            string str3;
            string str4;
            if (base.CollectionDataSet.Tables[DbMetaDataCollectionNames.DataSourceInformation] == null)
            {
                throw ADP.UnableToBuildCollection(DbMetaDataCollectionNames.DataSourceInformation);
            }
            DataTable table = base.CloneAndFilterCollection(DbMetaDataCollectionNames.DataSourceInformation, null);
            if (table.Rows.Count != 1)
            {
                throw ADP.IncorrectNumberOfDataSourceInformationRows();
            }
            DataRow row = table.Rows[0];
            string literalInfo = internalConnection.GetLiteralInfo(3);
            string unescapedString = internalConnection.GetLiteralInfo(0x1b);
            if (literalInfo != null)
            {
                StringBuilder builder3 = new StringBuilder();
                StringBuilder escapedString = new StringBuilder();
                ADP.EscapeSpecialCharacters(literalInfo, escapedString);
                builder3.Append(escapedString.ToString());
                if ((unescapedString != null) && (unescapedString != literalInfo))
                {
                    builder3.Append("|");
                    escapedString.Length = 0;
                    ADP.EscapeSpecialCharacters(unescapedString, escapedString);
                    builder3.Append(escapedString.ToString());
                }
                row[DbMetaDataColumnNames.CompositeIdentifierSeparatorPattern] = builder3.ToString();
            }
            else if (unescapedString != null)
            {
                StringBuilder builder4 = new StringBuilder();
                ADP.EscapeSpecialCharacters(unescapedString, builder4);
                row[DbMetaDataColumnNames.CompositeIdentifierSeparatorPattern] = builder4.ToString();
            }
            object dataSourcePropertyValue = connection.GetDataSourcePropertyValue(OleDbPropertySetGuid.DataSourceInfo, 40);
            if (dataSourcePropertyValue != null)
            {
                row[DbMetaDataColumnNames.DataSourceProductName] = (string) dataSourcePropertyValue;
            }
            row[DbMetaDataColumnNames.DataSourceProductVersion] = base.ServerVersion;
            row[DbMetaDataColumnNames.DataSourceProductVersionNormalized] = base.ServerVersionNormalized;
            row[DbMetaDataColumnNames.ParameterMarkerFormat] = "?";
            row[DbMetaDataColumnNames.ParameterMarkerPattern] = @"\?";
            row[DbMetaDataColumnNames.ParameterNameMaxLength] = 0;
            dataSourcePropertyValue = connection.GetDataSourcePropertyValue(OleDbPropertySetGuid.DataSourceInfo, 0x2c);
            GroupByBehavior unknown = GroupByBehavior.Unknown;
            if (dataSourcePropertyValue != null)
            {
                switch (((int) dataSourcePropertyValue))
                {
                    case 1:
                        unknown = GroupByBehavior.NotSupported;
                        break;

                    case 2:
                        unknown = GroupByBehavior.ExactMatch;
                        break;

                    case 4:
                        unknown = GroupByBehavior.MustContainAll;
                        break;

                    case 8:
                        unknown = GroupByBehavior.Unrelated;
                        break;
                }
            }
            row[DbMetaDataColumnNames.GroupByBehavior] = unknown;
            this.SetIdentifierCase(DbMetaDataColumnNames.IdentifierCase, 0x2e, row, connection);
            this.SetIdentifierCase(DbMetaDataColumnNames.QuotedIdentifierCase, 100, row, connection);
            dataSourcePropertyValue = connection.GetDataSourcePropertyValue(OleDbPropertySetGuid.DataSourceInfo, 0x55);
            if (dataSourcePropertyValue != null)
            {
                row[DbMetaDataColumnNames.OrderByColumnsInSelect] = (bool) dataSourcePropertyValue;
            }
            DataTable table2 = internalConnection.BuildInfoLiterals();
            if (table2 != null)
            {
                DataRow[] rowArray = table2.Select("Literal = " + 0x11.ToString(CultureInfo.InvariantCulture));
                if (rowArray != null)
                {
                    object obj4 = rowArray[0]["InvalidChars"];
                    if (obj4.GetType() == typeof(string))
                    {
                        string str6;
                        string invalidChars = (string) obj4;
                        object obj3 = rowArray[0]["InvalidStartingChars"];
                        if (obj3.GetType() == typeof(string))
                        {
                            str6 = (string) obj3;
                        }
                        else
                        {
                            str6 = invalidChars;
                        }
                        row[DbMetaDataColumnNames.IdentifierPattern] = this.BuildRegularExpression(invalidChars, str6);
                    }
                }
            }
            connection.GetLiteralQuotes("GetSchema", out str4, out str3);
            if (str4 != null)
            {
                if (str3 == null)
                {
                    str3 = str4;
                }
                if (str3.Length == 1)
                {
                    StringBuilder builder = new StringBuilder();
                    ADP.EscapeSpecialCharacters(str3, builder);
                    string str2 = builder.ToString();
                    builder.Length = 0;
                    ADP.EscapeSpecialCharacters(str4, builder);
                    builder.Append("(([^");
                    builder.Append(str2);
                    builder.Append("]|");
                    builder.Append(str2);
                    builder.Append(str2);
                    builder.Append(")*)");
                    builder.Append(str2);
                    row[DbMetaDataColumnNames.QuotedIdentifierPattern] = builder.ToString();
                }
            }
            table.AcceptChanges();
            return table;
        }

        private DataTable GetDataTypesTable(OleDbConnection connection)
        {
            if (base.CollectionDataSet.Tables[DbMetaDataCollectionNames.DataTypes] == null)
            {
                throw ADP.UnableToBuildCollection(DbMetaDataCollectionNames.DataTypes);
            }
            DataTable table = base.CloneAndFilterCollection(DbMetaDataCollectionNames.DataTypes, null);
            DataTable oleDbSchemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Provider_Types, null);
            DataColumn[] columnArray4 = new DataColumn[] { table.Columns[DbMetaDataColumnNames.TypeName], table.Columns[DbMetaDataColumnNames.ColumnSize], table.Columns[DbMetaDataColumnNames.CreateParameters], table.Columns[DbMetaDataColumnNames.IsAutoIncrementable], table.Columns[DbMetaDataColumnNames.IsCaseSensitive], table.Columns[DbMetaDataColumnNames.IsFixedLength], table.Columns[DbMetaDataColumnNames.IsFixedPrecisionScale], table.Columns[DbMetaDataColumnNames.IsLong], table.Columns[DbMetaDataColumnNames.IsNullable], table.Columns[DbMetaDataColumnNames.IsUnsigned], table.Columns[DbMetaDataColumnNames.MaximumScale], table.Columns[DbMetaDataColumnNames.MinimumScale], table.Columns[DbMetaDataColumnNames.LiteralPrefix], table.Columns[DbMetaDataColumnNames.LiteralSuffix], table.Columns[OleDbMetaDataColumnNames.NativeDataType] };
            DataColumn[] columnArray3 = new DataColumn[] { oleDbSchemaTable.Columns["TYPE_NAME"], oleDbSchemaTable.Columns["COLUMN_SIZE"], oleDbSchemaTable.Columns["CREATE_PARAMS"], oleDbSchemaTable.Columns["AUTO_UNIQUE_VALUE"], oleDbSchemaTable.Columns["CASE_SENSITIVE"], oleDbSchemaTable.Columns["IS_FIXEDLENGTH"], oleDbSchemaTable.Columns["FIXED_PREC_SCALE"], oleDbSchemaTable.Columns["IS_LONG"], oleDbSchemaTable.Columns["IS_NULLABLE"], oleDbSchemaTable.Columns["UNSIGNED_ATTRIBUTE"], oleDbSchemaTable.Columns["MAXIMUM_SCALE"], oleDbSchemaTable.Columns["MINIMUM_SCALE"], oleDbSchemaTable.Columns["LITERAL_PREFIX"], oleDbSchemaTable.Columns["LITERAL_SUFFIX"], oleDbSchemaTable.Columns["DATA_TYPE"] };
            DataColumn column2 = table.Columns[DbMetaDataColumnNames.IsSearchable];
            DataColumn column = table.Columns[DbMetaDataColumnNames.IsSearchableWithLike];
            DataColumn column8 = table.Columns[DbMetaDataColumnNames.ProviderDbType];
            DataColumn column7 = table.Columns[DbMetaDataColumnNames.DataType];
            DataColumn column6 = table.Columns[DbMetaDataColumnNames.IsLong];
            DataColumn column5 = table.Columns[DbMetaDataColumnNames.IsFixedLength];
            DataColumn column4 = oleDbSchemaTable.Columns["DATA_TYPE"];
            DataColumn column3 = oleDbSchemaTable.Columns["SEARCHABLE"];
            foreach (DataRow row2 in oleDbSchemaTable.Rows)
            {
                DataRow row = table.NewRow();
                for (int i = 0; i < columnArray3.Length; i++)
                {
                    if ((columnArray3[i] != null) && (columnArray4[i] != null))
                    {
                        row[columnArray4[i]] = row2[columnArray3[i]];
                    }
                }
                short dbType = (short) Convert.ChangeType(row2[column4], typeof(short), CultureInfo.InvariantCulture);
                NativeDBType type = NativeDBType.FromDBType(dbType, (bool) row[column6], (bool) row[column5]);
                row[column7] = type.dataType.FullName;
                row[column8] = type.enumOleDbType;
                if (((column2 != null) && (column != null)) && (column3 != null))
                {
                    row[column2] = DBNull.Value;
                    row[column] = DBNull.Value;
                    if (DBNull.Value != row2[column3])
                    {
                        long num2 = (long) row2[column3];
                        if ((num2 <= 4L) && (num2 >= 1L))
                        {
                            switch (((int) (num2 - 1L)))
                            {
                                case 0:
                                    row[column2] = false;
                                    row[column] = false;
                                    break;

                                case 1:
                                    row[column2] = false;
                                    row[column] = true;
                                    break;

                                case 2:
                                    row[column2] = true;
                                    row[column] = false;
                                    break;

                                case 3:
                                    row[column2] = true;
                                    row[column] = true;
                                    break;
                            }
                        }
                    }
                }
                table.Rows.Add(row);
            }
            table.AcceptChanges();
            return table;
        }

        private DataTable GetReservedWordsTable(OleDbConnectionInternal internalConnection)
        {
            if (base.CollectionDataSet.Tables[DbMetaDataCollectionNames.ReservedWords] == null)
            {
                throw ADP.UnableToBuildCollection(DbMetaDataCollectionNames.ReservedWords);
            }
            DataTable table = base.CloneAndFilterCollection(DbMetaDataCollectionNames.ReservedWords, null);
            DataColumn keyword = table.Columns[DbMetaDataColumnNames.ReservedWord];
            if (keyword == null)
            {
                throw ADP.UnableToBuildCollection(DbMetaDataCollectionNames.ReservedWords);
            }
            if (!internalConnection.AddInfoKeywordsToTable(table, keyword))
            {
                throw ODB.IDBInfoNotSupported();
            }
            return table;
        }

        protected override DataTable PrepareCollection(string collectionName, string[] restrictions, DbConnection connection)
        {
            OleDbConnection connection2 = (OleDbConnection) connection;
            OleDbConnectionInternal innerConnection = (OleDbConnectionInternal) connection2.InnerConnection;
            DataTable dataSourceInformationTable = null;
            if (collectionName == DbMetaDataCollectionNames.DataSourceInformation)
            {
                if (!ADP.IsEmptyArray(restrictions))
                {
                    throw ADP.TooManyRestrictions(DbMetaDataCollectionNames.DataSourceInformation);
                }
                dataSourceInformationTable = this.GetDataSourceInformationTable(connection2, innerConnection);
            }
            else if (collectionName == DbMetaDataCollectionNames.DataTypes)
            {
                if (!ADP.IsEmptyArray(restrictions))
                {
                    throw ADP.TooManyRestrictions(DbMetaDataCollectionNames.DataTypes);
                }
                dataSourceInformationTable = this.GetDataTypesTable(connection2);
            }
            else if (collectionName == DbMetaDataCollectionNames.ReservedWords)
            {
                if (!ADP.IsEmptyArray(restrictions))
                {
                    throw ADP.TooManyRestrictions(DbMetaDataCollectionNames.ReservedWords);
                }
                dataSourceInformationTable = this.GetReservedWordsTable(innerConnection);
            }
            else
            {
                for (int i = 0; i < this._schemaMapping.Length; i++)
                {
                    if (!(this._schemaMapping[i]._schemaName == collectionName))
                    {
                        continue;
                    }
                    object[] objArray = restrictions;
                    if (restrictions != null)
                    {
                        DataTable table2 = base.CollectionDataSet.Tables[DbMetaDataCollectionNames.MetaDataCollections];
                        int num6 = -1;
                        foreach (DataRow row in table2.Rows)
                        {
                            string str = (string) row[DbMetaDataColumnNames.CollectionName, DataRowVersion.Current];
                            if (collectionName == str)
                            {
                                num6 = (int) row[DbMetaDataColumnNames.NumberOfRestrictions];
                                if (num6 < restrictions.Length)
                                {
                                    throw ADP.TooManyRestrictions(collectionName);
                                }
                                break;
                            }
                        }
                        if (((collectionName == OleDbMetaDataCollectionNames.Indexes) && (restrictions.Length >= 4)) && (restrictions[3] != null))
                        {
                            ushort num4;
                            objArray = new object[restrictions.Length];
                            for (int j = 0; j < restrictions.Length; j++)
                            {
                                objArray[j] = restrictions[j];
                            }
                            if ((restrictions[3] == "DBPROPVAL_IT_BTREE") || (restrictions[3] == "1"))
                            {
                                num4 = 1;
                            }
                            else if ((restrictions[3] == "DBPROPVAL_IT_HASH") || (restrictions[3] == "2"))
                            {
                                num4 = 2;
                            }
                            else if ((restrictions[3] == "DBPROPVAL_IT_CONTENT") || (restrictions[3] == "3"))
                            {
                                num4 = 3;
                            }
                            else
                            {
                                if (!(restrictions[3] == "DBPROPVAL_IT_OTHER") && !(restrictions[3] == "4"))
                                {
                                    throw ADP.InvalidRestrictionValue(collectionName, "TYPE", restrictions[3]);
                                }
                                num4 = 4;
                            }
                            objArray[3] = num4;
                        }
                        if (((collectionName == OleDbMetaDataCollectionNames.Procedures) && (restrictions.Length >= 4)) && (restrictions[3] != null))
                        {
                            short num5;
                            objArray = new object[restrictions.Length];
                            for (int k = 0; k < restrictions.Length; k++)
                            {
                                objArray[k] = restrictions[k];
                            }
                            if ((restrictions[3] == "DB_PT_UNKNOWN") || (restrictions[3] == "1"))
                            {
                                num5 = 1;
                            }
                            else if ((restrictions[3] == "DB_PT_PROCEDURE") || (restrictions[3] == "2"))
                            {
                                num5 = 2;
                            }
                            else
                            {
                                if (!(restrictions[3] == "DB_PT_FUNCTION") && !(restrictions[3] == "3"))
                                {
                                    throw ADP.InvalidRestrictionValue(collectionName, "PROCEDURE_TYPE", restrictions[3]);
                                }
                                num5 = 3;
                            }
                            objArray[3] = num5;
                        }
                    }
                    dataSourceInformationTable = connection2.GetOleDbSchemaTable(this._schemaMapping[i]._schemaRowset, objArray);
                    break;
                }
            }
            if (dataSourceInformationTable == null)
            {
                throw ADP.UnableToBuildCollection(collectionName);
            }
            return dataSourceInformationTable;
        }

        private void SetIdentifierCase(string columnName, int propertyID, DataRow row, OleDbConnection connection)
        {
            object dataSourcePropertyValue = connection.GetDataSourcePropertyValue(OleDbPropertySetGuid.DataSourceInfo, propertyID);
            IdentifierCase unknown = IdentifierCase.Unknown;
            if (dataSourcePropertyValue != null)
            {
                switch (((int) dataSourcePropertyValue))
                {
                    case 1:
                    case 2:
                    case 8:
                        unknown = IdentifierCase.Insensitive;
                        break;

                    case 4:
                        unknown = IdentifierCase.Sensitive;
                        break;
                }
            }
            row[columnName] = unknown;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SchemaRowsetName
        {
            internal readonly string _schemaName;
            internal readonly Guid _schemaRowset;
            internal SchemaRowsetName(string schemaName, Guid schemaRowset)
            {
                this._schemaName = schemaName;
                this._schemaRowset = schemaRowset;
            }
        }
    }
}

