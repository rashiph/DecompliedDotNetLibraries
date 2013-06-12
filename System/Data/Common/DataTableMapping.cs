namespace System.Data.Common
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Data;
    using System.Globalization;

    [TypeConverter(typeof(DataTableMapping.DataTableMappingConverter))]
    public sealed class DataTableMapping : MarshalByRefObject, ITableMapping, ICloneable
    {
        private DataColumnMappingCollection _columnMappings;
        private string _dataSetTableName;
        private string _sourceTableName;
        private DataTableMappingCollection parent;

        public DataTableMapping()
        {
        }

        public DataTableMapping(string sourceTable, string dataSetTable)
        {
            this.SourceTable = sourceTable;
            this.DataSetTable = dataSetTable;
        }

        public DataTableMapping(string sourceTable, string dataSetTable, DataColumnMapping[] columnMappings)
        {
            this.SourceTable = sourceTable;
            this.DataSetTable = dataSetTable;
            if ((columnMappings != null) && (0 < columnMappings.Length))
            {
                this.ColumnMappings.AddRange(columnMappings);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public DataColumnMapping GetColumnMappingBySchemaAction(string sourceColumn, MissingMappingAction mappingAction)
        {
            return DataColumnMappingCollection.GetColumnMappingBySchemaAction(this._columnMappings, sourceColumn, mappingAction);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public DataColumn GetDataColumn(string sourceColumn, Type dataType, DataTable dataTable, MissingMappingAction mappingAction, MissingSchemaAction schemaAction)
        {
            return DataColumnMappingCollection.GetDataColumn(this._columnMappings, sourceColumn, dataType, dataTable, mappingAction, schemaAction);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public DataTable GetDataTableBySchemaAction(DataSet dataSet, MissingSchemaAction schemaAction)
        {
            if (dataSet == null)
            {
                throw ADP.ArgumentNull("dataSet");
            }
            string dataSetTable = this.DataSetTable;
            if (ADP.IsEmpty(dataSetTable))
            {
                return null;
            }
            DataTableCollection tables = dataSet.Tables;
            int index = tables.IndexOf(dataSetTable);
            if ((0 <= index) && (index < tables.Count))
            {
                return tables[index];
            }
            switch (schemaAction)
            {
                case MissingSchemaAction.Add:
                case MissingSchemaAction.AddWithKey:
                    return new DataTable(dataSetTable);

                case MissingSchemaAction.Ignore:
                    return null;

                case MissingSchemaAction.Error:
                    throw ADP.MissingTableSchema(dataSetTable, this.SourceTable);
            }
            throw ADP.InvalidMissingSchemaAction(schemaAction);
        }

        object ICloneable.Clone()
        {
            DataTableMapping mapping = new DataTableMapping {
                _dataSetTableName = this._dataSetTableName,
                _sourceTableName = this._sourceTableName
            };
            if ((this._columnMappings != null) && (0 < this.ColumnMappings.Count))
            {
                DataColumnMappingCollection columnMappings = mapping.ColumnMappings;
                foreach (ICloneable cloneable in this.ColumnMappings)
                {
                    columnMappings.Add(cloneable.Clone());
                }
            }
            return mapping;
        }

        public override string ToString()
        {
            return this.SourceTable;
        }

        [ResCategory("DataCategory_Mapping"), ResDescription("DataTableMapping_ColumnMappings"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DataColumnMappingCollection ColumnMappings
        {
            get
            {
                DataColumnMappingCollection mappings = this._columnMappings;
                if (mappings == null)
                {
                    mappings = new DataColumnMappingCollection();
                    this._columnMappings = mappings;
                }
                return mappings;
            }
        }

        [ResDescription("DataTableMapping_DataSetTable"), ResCategory("DataCategory_Mapping"), DefaultValue("")]
        public string DataSetTable
        {
            get
            {
                string str = this._dataSetTableName;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                this._dataSetTableName = value;
            }
        }

        internal DataTableMappingCollection Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        [DefaultValue(""), ResDescription("DataTableMapping_SourceTable"), ResCategory("DataCategory_Mapping")]
        public string SourceTable
        {
            get
            {
                string str = this._sourceTableName;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                if ((this.Parent != null) && (ADP.SrcCompare(this._sourceTableName, value) != 0))
                {
                    this.Parent.ValidateSourceTable(-1, value);
                }
                this._sourceTableName = value;
            }
        }

        IColumnMappingCollection ITableMapping.ColumnMappings
        {
            get
            {
                return this.ColumnMappings;
            }
        }

        internal sealed class DataTableMappingConverter : ExpandableObjectConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return ((typeof(InstanceDescriptor) == destinationType) || base.CanConvertTo(context, destinationType));
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (null == destinationType)
                {
                    throw ADP.ArgumentNull("destinationType");
                }
                if ((typeof(InstanceDescriptor) == destinationType) && (value is DataTableMapping))
                {
                    DataTableMapping mapping = (DataTableMapping) value;
                    DataColumnMapping[] array = new DataColumnMapping[mapping.ColumnMappings.Count];
                    mapping.ColumnMappings.CopyTo(array, 0);
                    object[] arguments = new object[] { mapping.SourceTable, mapping.DataSetTable, array };
                    Type[] types = new Type[] { typeof(string), typeof(string), typeof(DataColumnMapping[]) };
                    return new InstanceDescriptor(typeof(DataTableMapping).GetConstructor(types), arguments);
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}

