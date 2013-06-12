namespace System.Data.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.IO;
    using System.Text;

    internal sealed class SqlMetaDataFactory : DbMetaDataFactory
    {
        private const string _serverVersionNormalized10 = "10.00.0000";
        private const string _serverVersionNormalized90 = "09.00.0000";
        private const string _serverVersionNormalized90782 = "09.00.0782";

        public SqlMetaDataFactory(Stream XMLStream, string serverVersion, string serverVersionNormalized) : base(XMLStream, serverVersion, serverVersionNormalized)
        {
        }

        private void AddTVPsToDataTypesTable(DataTable dataTypesTable, SqlConnection connection, string ServerVersion)
        {
            if (0 <= string.Compare(ServerVersion, "10.00.0000", StringComparison.OrdinalIgnoreCase))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = "select name, is_nullable, max_length from sys.types where is_table_type = 1";
                DataRow row = null;
                DataColumn column6 = dataTypesTable.Columns[DbMetaDataColumnNames.ProviderDbType];
                DataColumn column5 = dataTypesTable.Columns[DbMetaDataColumnNames.ColumnSize];
                DataColumn column4 = dataTypesTable.Columns[DbMetaDataColumnNames.IsSearchable];
                DataColumn column3 = dataTypesTable.Columns[DbMetaDataColumnNames.IsLiteralSupported];
                DataColumn column2 = dataTypesTable.Columns[DbMetaDataColumnNames.TypeName];
                DataColumn column = dataTypesTable.Columns[DbMetaDataColumnNames.IsNullable];
                if ((((column6 == null) || (column5 == null)) || ((column4 == null) || (column3 == null))) || ((column2 == null) || (column == null)))
                {
                    throw ADP.InvalidXml();
                }
                using (IDataReader reader = command.ExecuteReader())
                {
                    object[] values = new object[11];
                    while (reader.Read())
                    {
                        reader.GetValues(values);
                        row = dataTypesTable.NewRow();
                        row[column6] = SqlDbType.Structured;
                        if (values[2] != DBNull.Value)
                        {
                            row[column5] = values[2];
                        }
                        row[column4] = false;
                        row[column3] = false;
                        if (values[1] != DBNull.Value)
                        {
                            row[column] = values[1];
                        }
                        if (values[0] != DBNull.Value)
                        {
                            row[column2] = values[0];
                            dataTypesTable.Rows.Add(row);
                            row.AcceptChanges();
                        }
                    }
                }
            }
        }

        private void addUDTsToDataTypesTable(DataTable dataTypesTable, SqlConnection connection, string ServerVersion)
        {
            if (0 <= string.Compare(ServerVersion, "09.00.0000", StringComparison.OrdinalIgnoreCase))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = "select assemblies.name, types.assembly_class, ASSEMBLYPROPERTY(assemblies.name, 'VersionMajor') as version_major, ASSEMBLYPROPERTY(assemblies.name, 'VersionMinor') as version_minor, ASSEMBLYPROPERTY(assemblies.name, 'VersionBuild') as version_build, ASSEMBLYPROPERTY(assemblies.name, 'VersionRevision') as version_revision, ASSEMBLYPROPERTY(assemblies.name, 'CultureInfo') as culture_info, ASSEMBLYPROPERTY(assemblies.name, 'PublicKey') as public_key, is_nullable, is_fixed_length, max_length from sys.assemblies as assemblies  join sys.assembly_types as types on assemblies.assembly_id = types.assembly_id ";
                DataRow row = null;
                DataColumn column7 = dataTypesTable.Columns[DbMetaDataColumnNames.ProviderDbType];
                DataColumn column6 = dataTypesTable.Columns[DbMetaDataColumnNames.ColumnSize];
                DataColumn column5 = dataTypesTable.Columns[DbMetaDataColumnNames.IsFixedLength];
                DataColumn column4 = dataTypesTable.Columns[DbMetaDataColumnNames.IsSearchable];
                DataColumn column3 = dataTypesTable.Columns[DbMetaDataColumnNames.IsLiteralSupported];
                DataColumn column2 = dataTypesTable.Columns[DbMetaDataColumnNames.TypeName];
                DataColumn column = dataTypesTable.Columns[DbMetaDataColumnNames.IsNullable];
                if ((((column7 == null) || (column6 == null)) || ((column5 == null) || (column4 == null))) || (((column3 == null) || (column2 == null)) || (column == null)))
                {
                    throw ADP.InvalidXml();
                }
                using (IDataReader reader = command.ExecuteReader())
                {
                    object[] values = new object[11];
                    while (reader.Read())
                    {
                        reader.GetValues(values);
                        row = dataTypesTable.NewRow();
                        row[column7] = SqlDbType.Udt;
                        if (values[10] != DBNull.Value)
                        {
                            row[column6] = values[10];
                        }
                        if (values[9] != DBNull.Value)
                        {
                            row[column5] = values[9];
                        }
                        row[column4] = true;
                        row[column3] = false;
                        if (values[8] != DBNull.Value)
                        {
                            row[column] = values[8];
                        }
                        if ((((values[0] != DBNull.Value) && (values[1] != DBNull.Value)) && ((values[2] != DBNull.Value) && (values[3] != DBNull.Value))) && ((values[4] != DBNull.Value) && (values[5] != DBNull.Value)))
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.Append(values[1].ToString());
                            builder.Append(", ");
                            builder.Append(values[0].ToString());
                            builder.Append(", Version=");
                            builder.Append(values[2].ToString());
                            builder.Append(".");
                            builder.Append(values[3].ToString());
                            builder.Append(".");
                            builder.Append(values[4].ToString());
                            builder.Append(".");
                            builder.Append(values[5].ToString());
                            if (values[6] != DBNull.Value)
                            {
                                builder.Append(", Culture=");
                                builder.Append(values[6].ToString());
                            }
                            if (values[7] != DBNull.Value)
                            {
                                builder.Append(", PublicKeyToken=");
                                StringBuilder builder2 = new StringBuilder();
                                byte[] buffer2 = (byte[]) values[7];
                                foreach (byte num2 in buffer2)
                                {
                                    builder2.Append(string.Format(null, "{0,-2:x2}", new object[] { num2 }));
                                }
                                builder.Append(builder2.ToString());
                            }
                            row[column2] = builder.ToString();
                            dataTypesTable.Rows.Add(row);
                            row.AcceptChanges();
                        }
                    }
                }
            }
        }

        private DataTable GetDataTypesTable(SqlConnection connection)
        {
            if (base.CollectionDataSet.Tables[DbMetaDataCollectionNames.DataTypes] == null)
            {
                throw ADP.UnableToBuildCollection(DbMetaDataCollectionNames.DataTypes);
            }
            DataTable dataTypesTable = base.CloneAndFilterCollection(DbMetaDataCollectionNames.DataTypes, null);
            this.addUDTsToDataTypesTable(dataTypesTable, connection, base.ServerVersionNormalized);
            this.AddTVPsToDataTypesTable(dataTypesTable, connection, base.ServerVersionNormalized);
            dataTypesTable.AcceptChanges();
            return dataTypesTable;
        }

        protected override DataTable PrepareCollection(string collectionName, string[] restrictions, DbConnection connection)
        {
            SqlConnection connection2 = (SqlConnection) connection;
            DataTable dataTypesTable = null;
            if (collectionName == DbMetaDataCollectionNames.DataTypes)
            {
                if (!ADP.IsEmptyArray(restrictions))
                {
                    throw ADP.TooManyRestrictions(DbMetaDataCollectionNames.DataTypes);
                }
                dataTypesTable = this.GetDataTypesTable(connection2);
            }
            if (dataTypesTable == null)
            {
                throw ADP.UnableToBuildCollection(collectionName);
            }
            return dataTypesTable;
        }
    }
}

