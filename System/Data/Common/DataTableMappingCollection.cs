namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Reflection;

    [ListBindable(false), Editor("Microsoft.VSDesigner.Data.Design.DataTableMappingCollectionEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public sealed class DataTableMappingCollection : MarshalByRefObject, ITableMappingCollection, IList, ICollection, IEnumerable
    {
        private List<DataTableMapping> items;

        private DataTableMapping Add(DataTableMapping value)
        {
            this.AddWithoutEvents(value);
            return value;
        }

        public int Add(object value)
        {
            this.ValidateType(value);
            this.Add((DataTableMapping) value);
            return (this.Count - 1);
        }

        public DataTableMapping Add(string sourceTable, string dataSetTable)
        {
            return this.Add(new DataTableMapping(sourceTable, dataSetTable));
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
                    this.AddWithoutEvents(cloneable.Clone() as DataTableMapping);
                }
            }
            else
            {
                foreach (DataTableMapping mapping in values)
                {
                    this.AddWithoutEvents(mapping);
                }
            }
        }

        public void AddRange(DataTableMapping[] values)
        {
            this.AddEnumerableRange(values, false);
        }

        public void AddRange(Array values)
        {
            this.AddEnumerableRange(values, false);
        }

        private void AddWithoutEvents(DataTableMapping value)
        {
            this.Validate(-1, value);
            value.Parent = this;
            this.ArrayList().Add(value);
        }

        private List<DataTableMapping> ArrayList()
        {
            if (this.items == null)
            {
                this.items = new List<DataTableMapping>();
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
                foreach (DataTableMapping mapping in this.items)
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

        public void CopyTo(DataTableMapping[] array, int index)
        {
            this.ArrayList().CopyTo(array, index);
        }

        public DataTableMapping GetByDataSetTable(string dataSetTable)
        {
            int num = this.IndexOfDataSetTable(dataSetTable);
            if (0 > num)
            {
                throw ADP.TablesDataSetTable(dataSetTable);
            }
            return this.items[num];
        }

        public IEnumerator GetEnumerator()
        {
            return this.ArrayList().GetEnumerator();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataTableMapping GetTableMappingBySchemaAction(DataTableMappingCollection tableMappings, string sourceTable, string dataSetTable, MissingMappingAction mappingAction)
        {
            if (tableMappings != null)
            {
                int index = tableMappings.IndexOf(sourceTable);
                if (-1 != index)
                {
                    return tableMappings.items[index];
                }
            }
            if (ADP.IsEmpty(sourceTable))
            {
                throw ADP.InvalidSourceTable("sourceTable");
            }
            switch (mappingAction)
            {
                case MissingMappingAction.Passthrough:
                    return new DataTableMapping(sourceTable, dataSetTable);

                case MissingMappingAction.Ignore:
                    return null;

                case MissingMappingAction.Error:
                    throw ADP.MissingTableMapping(sourceTable);
            }
            throw ADP.InvalidMissingMappingAction(mappingAction);
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

        public int IndexOf(string sourceTable)
        {
            if (!ADP.IsEmpty(sourceTable))
            {
                for (int i = 0; i < this.Count; i++)
                {
                    string strB = this.items[i].SourceTable;
                    if ((strB != null) && (ADP.SrcCompare(sourceTable, strB) == 0))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public int IndexOfDataSetTable(string dataSetTable)
        {
            if (!ADP.IsEmpty(dataSetTable))
            {
                for (int i = 0; i < this.Count; i++)
                {
                    string strB = this.items[i].DataSetTable;
                    if ((strB != null) && (ADP.DstCompare(dataSetTable, strB) == 0))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public void Insert(int index, DataTableMapping value)
        {
            if (value == null)
            {
                throw ADP.TablesAddNullAttempt("value");
            }
            this.Validate(-1, value);
            value.Parent = this;
            this.ArrayList().Insert(index, value);
        }

        public void Insert(int index, object value)
        {
            this.ValidateType(value);
            this.Insert(index, (DataTableMapping) value);
        }

        private void RangeCheck(int index)
        {
            if ((index < 0) || (this.Count <= index))
            {
                throw ADP.TablesIndexInt32(index, this);
            }
        }

        private int RangeCheck(string sourceTable)
        {
            int index = this.IndexOf(sourceTable);
            if (index < 0)
            {
                throw ADP.TablesSourceIndex(sourceTable);
            }
            return index;
        }

        public void Remove(DataTableMapping value)
        {
            if (value == null)
            {
                throw ADP.TablesAddNullAttempt("value");
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
            this.Remove((DataTableMapping) value);
        }

        public void RemoveAt(int index)
        {
            this.RangeCheck(index);
            this.RemoveIndex(index);
        }

        public void RemoveAt(string sourceTable)
        {
            int index = this.RangeCheck(sourceTable);
            this.RemoveIndex(index);
        }

        private void RemoveIndex(int index)
        {
            this.items[index].Parent = null;
            this.items.RemoveAt(index);
        }

        private void Replace(int index, DataTableMapping newValue)
        {
            this.Validate(index, newValue);
            this.items[index].Parent = null;
            newValue.Parent = this;
            this.items[index] = newValue;
        }

        ITableMapping ITableMappingCollection.Add(string sourceTableName, string dataSetTableName)
        {
            return this.Add(sourceTableName, dataSetTableName);
        }

        ITableMapping ITableMappingCollection.GetByDataSetTable(string dataSetTableName)
        {
            return this.GetByDataSetTable(dataSetTableName);
        }

        private void Validate(int index, DataTableMapping value)
        {
            if (value == null)
            {
                throw ADP.TablesAddNullAttempt("value");
            }
            if (value.Parent != null)
            {
                if (this != value.Parent)
                {
                    throw ADP.TablesIsNotParent(this);
                }
                if (index != this.IndexOf(value))
                {
                    throw ADP.TablesIsParent(this);
                }
            }
            string sourceTable = value.SourceTable;
            if (ADP.IsEmpty(sourceTable))
            {
                index = 1;
                do
                {
                    sourceTable = "SourceTable" + index.ToString(CultureInfo.InvariantCulture);
                    index++;
                }
                while (-1 != this.IndexOf(sourceTable));
                value.SourceTable = sourceTable;
            }
            else
            {
                this.ValidateSourceTable(index, sourceTable);
            }
        }

        internal void ValidateSourceTable(int index, string value)
        {
            int num = this.IndexOf(value);
            if ((-1 != num) && (index != num))
            {
                throw ADP.TablesUniqueSourceTable(value);
            }
        }

        private void ValidateType(object value)
        {
            if (value == null)
            {
                throw ADP.TablesAddNullAttempt("value");
            }
            if (!this.ItemType.IsInstanceOfType(value))
            {
                throw ADP.NotADataTableMapping(value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResDescription("DataTableMappings_Count"), Browsable(false)]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResDescription("DataTableMappings_Item"), Browsable(false)]
        public DataTableMapping this[int index]
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

        [ResDescription("DataTableMappings_Item"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataTableMapping this[string sourceTable]
        {
            get
            {
                int num = this.RangeCheck(sourceTable);
                return this.items[num];
            }
            set
            {
                int index = this.RangeCheck(sourceTable);
                this.Replace(index, value);
            }
        }

        private Type ItemType
        {
            get
            {
                return typeof(DataTableMapping);
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
                this[index] = (DataTableMapping) value;
            }
        }

        object ITableMappingCollection.this[string index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this.ValidateType(value);
                this[index] = (DataTableMapping) value;
            }
        }
    }
}

