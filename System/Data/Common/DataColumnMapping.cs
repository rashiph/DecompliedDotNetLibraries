namespace System.Data.Common
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Data;
    using System.Globalization;

    [TypeConverter(typeof(DataColumnMapping.DataColumnMappingConverter))]
    public sealed class DataColumnMapping : MarshalByRefObject, IColumnMapping, ICloneable
    {
        private string _dataSetColumnName;
        private string _sourceColumnName;
        private DataColumnMappingCollection parent;

        public DataColumnMapping()
        {
        }

        public DataColumnMapping(string sourceColumn, string dataSetColumn)
        {
            this.SourceColumn = sourceColumn;
            this.DataSetColumn = dataSetColumn;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public DataColumn GetDataColumnBySchemaAction(DataTable dataTable, Type dataType, MissingSchemaAction schemaAction)
        {
            return GetDataColumnBySchemaAction(this.SourceColumn, this.DataSetColumn, dataTable, dataType, schemaAction);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataColumn GetDataColumnBySchemaAction(string sourceColumn, string dataSetColumn, DataTable dataTable, Type dataType, MissingSchemaAction schemaAction)
        {
            if (dataTable == null)
            {
                throw ADP.ArgumentNull("dataTable");
            }
            if (ADP.IsEmpty(dataSetColumn))
            {
                return null;
            }
            DataColumnCollection columns = dataTable.Columns;
            int index = columns.IndexOf(dataSetColumn);
            if ((0 <= index) && (index < columns.Count))
            {
                DataColumn column = columns[index];
                if (!ADP.IsEmpty(column.Expression))
                {
                    throw ADP.ColumnSchemaExpression(sourceColumn, dataSetColumn);
                }
                if ((null != dataType) && (dataType.IsArray != column.DataType.IsArray))
                {
                    throw ADP.ColumnSchemaMismatch(sourceColumn, dataType, column);
                }
                return column;
            }
            switch (schemaAction)
            {
                case MissingSchemaAction.Add:
                case MissingSchemaAction.AddWithKey:
                    return new DataColumn(dataSetColumn, dataType);

                case MissingSchemaAction.Ignore:
                    return null;

                case MissingSchemaAction.Error:
                    throw ADP.ColumnSchemaMissing(dataSetColumn, dataTable.TableName, sourceColumn);
            }
            throw ADP.InvalidMissingSchemaAction(schemaAction);
        }

        object ICloneable.Clone()
        {
            return new DataColumnMapping { _sourceColumnName = this._sourceColumnName, _dataSetColumnName = this._dataSetColumnName };
        }

        public override string ToString()
        {
            return this.SourceColumn;
        }

        [ResCategory("DataCategory_Mapping"), ResDescription("DataColumnMapping_DataSetColumn"), DefaultValue("")]
        public string DataSetColumn
        {
            get
            {
                string str = this._dataSetColumnName;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                this._dataSetColumnName = value;
            }
        }

        internal DataColumnMappingCollection Parent
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

        [DefaultValue(""), ResCategory("DataCategory_Mapping"), ResDescription("DataColumnMapping_SourceColumn")]
        public string SourceColumn
        {
            get
            {
                string str = this._sourceColumnName;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                if ((this.Parent != null) && (ADP.SrcCompare(this._sourceColumnName, value) != 0))
                {
                    this.Parent.ValidateSourceColumn(-1, value);
                }
                this._sourceColumnName = value;
            }
        }

        internal sealed class DataColumnMappingConverter : ExpandableObjectConverter
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
                if ((typeof(InstanceDescriptor) == destinationType) && (value is DataColumnMapping))
                {
                    DataColumnMapping mapping = (DataColumnMapping) value;
                    object[] arguments = new object[] { mapping.SourceColumn, mapping.DataSetColumn };
                    Type[] types = new Type[] { typeof(string), typeof(string) };
                    return new InstanceDescriptor(typeof(DataColumnMapping).GetConstructor(types), arguments);
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}

