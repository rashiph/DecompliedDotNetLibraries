namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Reflection;

    public sealed class DataColumnMappingCollection : MarshalByRefObject, IColumnMappingCollection, IList, ICollection, IEnumerable
    {
        private List<DataColumnMapping> items;

        private DataColumnMapping Add(DataColumnMapping value)
        {
            this.AddWithoutEvents(value);
            return value;
        }

        public int Add(object value)
        {
            this.ValidateType(value);
            this.Add((DataColumnMapping) value);
            return (this.Count - 1);
        }

        public DataColumnMapping Add(string sourceColumn, string dataSetColumn)
        {
            return this.Add(new DataColumnMapping(sourceColumn, dataSetColumn));
        }

        private void AddEnumerableRange(IEnumerable values, bool doClone)
        {
            if (values == null)
            {
                throw ADP.ArgumentNull("values");
            }
            foreach (object obj2 in values)
            {
                this.ValidateType(obj2);
            }
            if (doClone)
            {
                foreach (ICloneable cloneable in values)
                {
                    this.AddWithoutEvents(cloneable.Clone() as DataColumnMapping);
                }
            }
            else
            {
                foreach (DataColumnMapping mapping in values)
                {
                    this.AddWithoutEvents(mapping);
                }
            }
        }

        public void AddRange(DataColumnMapping[] values)
        {
            this.AddEnumerableRange(values, false);
        }

        public void AddRange(Array values)
        {
            this.AddEnumerableRange(values, false);
        }

        private void AddWithoutEvents(DataColumnMapping value)
        {
            this.Validate(-1, value);
            value.Parent = this;
            this.ArrayList().Add(value);
        }

        private List<DataColumnMapping> ArrayList()
        {
            if (this.items == null)
            {
                this.items = new List<DataColumnMapping>();
            }
            return this.items;
        }

        public void Clear()
        {
            if (0 < this.Count)
            {
                this.ClearWithoutEvents();
            }
        }

        private void ClearWithoutEvents()
        {
            if (this.items != null)
            {
                foreach (DataColumnMapping mapping in this.items)
                {
                    mapping.Parent = null;
                }
                this.items.Clear();
            }
        }

        public bool Contains(object value)
        {
            return (-1 != this.IndexOf(value));
        }

        public bool Contains(string value)
        {
            return (-1 != this.IndexOf(value));
        }

        public void CopyTo(Array array, int index)
        {
            this.ArrayList().CopyTo(array, index);
        }

        public void CopyTo(DataColumnMapping[] array, int index)
        {
            this.ArrayList().CopyTo(array, index);
        }

        public DataColumnMapping GetByDataSetColumn(string value)
        {
            int num = this.IndexOfDataSetColumn(value);
            if (0 > num)
            {
                throw ADP.ColumnsDataSetColumn(value);
            }
            return this.items[num];
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataColumnMapping GetColumnMappingBySchemaAction(DataColumnMappingCollection columnMappings, string sourceColumn, MissingMappingAction mappingAction)
        {
            if (columnMappings != null)
            {
                int index = columnMappings.IndexOf(sourceColumn);
                if (-1 != index)
                {
                    return columnMappings.items[index];
                }
            }
            if (ADP.IsEmpty(sourceColumn))
            {
                throw ADP.InvalidSourceColumn("sourceColumn");
            }
            switch (mappingAction)
            {
                case MissingMappingAction.Passthrough:
                    return new DataColumnMapping(sourceColumn, sourceColumn);

                case MissingMappingAction.Ignore:
                    return null;

                case MissingMappingAction.Error:
                    throw ADP.MissingColumnMapping(sourceColumn);
            }
            throw ADP.InvalidMissingMappingAction(mappingAction);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataColumn GetDataColumn(DataColumnMappingCollection columnMappings, string sourceColumn, Type dataType, DataTable dataTable, MissingMappingAction mappingAction, MissingSchemaAction schemaAction)
        {
            if (columnMappings != null)
            {
                int index = columnMappings.IndexOf(sourceColumn);
                if (-1 != index)
                {
                    return columnMappings.items[index].GetDataColumnBySchemaAction(dataTable, dataType, schemaAction);
                }
            }
            if (ADP.IsEmpty(sourceColumn))
            {
                throw ADP.InvalidSourceColumn("sourceColumn");
            }
            switch (mappingAction)
            {
                case MissingMappingAction.Passthrough:
                    return DataColumnMapping.GetDataColumnBySchemaAction(sourceColumn, sourceColumn, dataTable, dataType, schemaAction);

                case MissingMappingAction.Ignore:
                    return null;

                case MissingMappingAction.Error:
                    throw ADP.MissingColumnMapping(sourceColumn);
            }
            throw ADP.InvalidMissingMappingAction(mappingAction);
        }

        public IEnumerator GetEnumerator()
        {
            return this.ArrayList().GetEnumerator();
        }

        public int IndexOf(object value)
        {
            if (value != null)
            {
                this.ValidateType(value);
                for (int i = 0; i < this.Count; i++)
                {
                    if (this.items[i] == value)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public int IndexOf(string sourceColumn)
        {
            if (!ADP.IsEmpty(sourceColumn))
            {
                int count = this.Count;
                for (int i = 0; i < count; i++)
                {
                    if (ADP.SrcCompare(sourceColumn, this.items[i].SourceColumn) == 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public int IndexOfDataSetColumn(string dataSetColumn)
        {
            if (!ADP.IsEmpty(dataSetColumn))
            {
                int count = this.Count;
                for (int i = 0; i < count; i++)
                {
                    if (ADP.DstCompare(dataSetColumn, this.items[i].DataSetColumn) == 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public void Insert(int index, DataColumnMapping value)
        {
            if (value == null)
            {
                throw ADP.ColumnsAddNullAttempt("value");
            }
            this.Validate(-1, value);
            value.Parent = this;
            this.ArrayList().Insert(index, value);
        }

        public void Insert(int index, object value)
        {
            this.ValidateType(value);
            this.Insert(index, (DataColumnMapping) value);
        }

        private void RangeCheck(int index)
        {
            if ((index < 0) || (this.Count <= index))
            {
                throw ADP.ColumnsIndexInt32(index, this);
            }
        }

        private int RangeCheck(string sourceColumn)
        {
            int index = this.IndexOf(sourceColumn);
            if (index < 0)
            {
                throw ADP.ColumnsIndexSource(sourceColumn);
            }
            return index;
        }

        public void Remove(DataColumnMapping value)
        {
            if (value == null)
            {
                throw ADP.ColumnsAddNullAttempt("value");
            }
            int index = this.IndexOf(value);
            if (-1 == index)
            {
                throw ADP.CollectionRemoveInvalidObject(this.ItemType, this);
            }
            this.RemoveIndex(index);
        }

        public void Remove(object value)
        {
            this.ValidateType(value);
            this.Remove((DataColumnMapping) value);
        }

        public void RemoveAt(int index)
        {
            this.RangeCheck(index);
            this.RemoveIndex(index);
        }

        public void RemoveAt(string sourceColumn)
        {
            int index = this.RangeCheck(sourceColumn);
            this.RemoveIndex(index);
        }

        private void RemoveIndex(int index)
        {
            this.items[index].Parent = null;
            this.items.RemoveAt(index);
        }

        private void Replace(int index, DataColumnMapping newValue)
        {
            this.Validate(index, newValue);
            this.items[index].Parent = null;
            newValue.Parent = this;
            this.items[index] = newValue;
        }

        IColumnMapping IColumnMappingCollection.Add(string sourceColumnName, string dataSetColumnName)
        {
            return this.Add(sourceColumnName, dataSetColumnName);
        }

        IColumnMapping IColumnMappingCollection.GetByDataSetColumn(string dataSetColumnName)
        {
            return this.GetByDataSetColumn(dataSetColumnName);
        }

        private void Validate(int index, DataColumnMapping value)
        {
            if (value == null)
            {
                throw ADP.ColumnsAddNullAttempt("value");
            }
            if (value.Parent != null)
            {
                if (this != value.Parent)
                {
                    throw ADP.ColumnsIsNotParent(this);
                }
                if (index != this.IndexOf(value))
                {
                    throw ADP.ColumnsIsParent(this);
                }
            }
            string sourceColumn = value.SourceColumn;
            if (ADP.IsEmpty(sourceColumn))
            {
                index = 1;
                do
                {
                    sourceColumn = "SourceColumn" + index.ToString(CultureInfo.InvariantCulture);
                    index++;
                }
                while (-1 != this.IndexOf(sourceColumn));
                value.SourceColumn = sourceColumn;
            }
            else
            {
                this.ValidateSourceColumn(index, sourceColumn);
            }
        }

        internal void ValidateSourceColumn(int index, string value)
        {
            int num = this.IndexOf(value);
            if ((-1 != num) && (index != num))
            {
                throw ADP.ColumnsUniqueSourceColumn(value);
            }
        }

        private void ValidateType(object value)
        {
            if (value == null)
            {
                throw ADP.ColumnsAddNullAttempt("value");
            }
            if (!this.ItemType.IsInstanceOfType(value))
            {
                throw ADP.NotADataColumnMapping(value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), ResDescription("DataColumnMappings_Count")]
        public int Count
        {
            get
            {
                if (this.items == null)
                {
                    return 0;
                }
                return this.items.Count;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResDescription("DataColumnMappings_Item")]
        public DataColumnMapping this[int index]
        {
            get
            {
                this.RangeCheck(index);
                return this.items[index];
            }
            set
            {
                this.RangeCheck(index);
                this.Replace(index, value);
            }
        }

        [ResDescription("DataColumnMappings_Item"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataColumnMapping this[string sourceColumn]
        {
            get
            {
                int num = this.RangeCheck(sourceColumn);
                return this.items[num];
            }
            set
            {
                int index = this.RangeCheck(sourceColumn);
                this.Replace(index, value);
            }
        }

        private Type ItemType
        {
            get
            {
                return typeof(DataColumnMapping);
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this.ValidateType(value);
                this[index] = (DataColumnMapping) value;
            }
        }

        object IColumnMappingCollection.this[string index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this.ValidateType(value);
                this[index] = (DataColumnMapping) value;
            }
        }
    }
}

