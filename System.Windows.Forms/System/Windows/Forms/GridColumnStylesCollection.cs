namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;

    [ListBindable(false), Editor("System.Windows.Forms.Design.DataGridColumnCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public class GridColumnStylesCollection : BaseCollection, IList, ICollection, IEnumerable
    {
        private bool isDefault;
        private ArrayList items;
        private System.Windows.Forms.DataGridTableStyle owner;

        public event CollectionChangeEventHandler CollectionChanged;

        internal GridColumnStylesCollection(System.Windows.Forms.DataGridTableStyle table)
        {
            this.items = new ArrayList();
            this.owner = table;
        }

        internal GridColumnStylesCollection(System.Windows.Forms.DataGridTableStyle table, bool isDefault) : this(table)
        {
            this.isDefault = isDefault;
        }

        public virtual int Add(DataGridColumnStyle column)
        {
            if (this.isDefault)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultColumnCollectionChanged"));
            }
            this.CheckForMappingNameDuplicates(column);
            column.SetDataGridTableInColumn(this.owner, true);
            column.MappingNameChanged += new EventHandler(this.ColumnStyleMappingNameChanged);
            column.PropertyDescriptorChanged += new EventHandler(this.ColumnStylePropDescChanged);
            if ((this.DataGridTableStyle != null) && (column.Width == -1))
            {
                column.width = this.DataGridTableStyle.PreferredColumnWidth;
            }
            int num = this.items.Add(column);
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, column));
            return num;
        }

        internal void AddDefaultColumn(DataGridColumnStyle column)
        {
            column.SetDataGridTableInColumn(this.owner, true);
            this.items.Add(column);
        }

        public void AddRange(DataGridColumnStyle[] columns)
        {
            if (columns == null)
            {
                throw new ArgumentNullException("columns");
            }
            for (int i = 0; i < columns.Length; i++)
            {
                this.Add(columns[i]);
            }
        }

        internal void CheckForMappingNameDuplicates(DataGridColumnStyle column)
        {
            if (!string.IsNullOrEmpty(column.MappingName))
            {
                for (int i = 0; i < this.items.Count; i++)
                {
                    if (((DataGridColumnStyle) this.items[i]).MappingName.Equals(column.MappingName) && (column != this.items[i]))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridColumnStyleDuplicateMappingName"), "column");
                    }
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].ReleaseHostedControl();
            }
            this.items.Clear();
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        private void ColumnStyleMappingNameChanged(object sender, EventArgs pcea)
        {
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        private void ColumnStylePropDescChanged(object sender, EventArgs pcea)
        {
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, (DataGridColumnStyle) sender));
        }

        public bool Contains(PropertyDescriptor propertyDescriptor)
        {
            return (this[propertyDescriptor] != null);
        }

        public bool Contains(string name)
        {
            IEnumerator enumerator = this.items.GetEnumerator();
            while (enumerator.MoveNext())
            {
                DataGridColumnStyle current = (DataGridColumnStyle) enumerator.Current;
                if (string.Compare(current.MappingName, name, true, CultureInfo.InvariantCulture) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(DataGridColumnStyle column)
        {
            return (this.items.IndexOf(column) != -1);
        }

        public int IndexOf(DataGridColumnStyle element)
        {
            int count = this.items.Count;
            for (int i = 0; i < count; i++)
            {
                DataGridColumnStyle style = (DataGridColumnStyle) this.items[i];
                if (element == style)
                {
                    return i;
                }
            }
            return -1;
        }

        internal DataGridColumnStyle MapColumnStyleToPropertyName(string mappingName)
        {
            int count = this.items.Count;
            for (int i = 0; i < count; i++)
            {
                DataGridColumnStyle style = (DataGridColumnStyle) this.items[i];
                if (string.Equals(style.MappingName, mappingName, StringComparison.OrdinalIgnoreCase))
                {
                    return style;
                }
            }
            return null;
        }

        protected void OnCollectionChanged(CollectionChangeEventArgs e)
        {
            if (this.onCollectionChanged != null)
            {
                this.onCollectionChanged(this, e);
            }
            DataGrid dataGrid = this.owner.DataGrid;
            if (dataGrid != null)
            {
                dataGrid.checkHierarchy = true;
            }
        }

        public void Remove(DataGridColumnStyle column)
        {
            if (this.isDefault)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultColumnCollectionChanged"));
            }
            int index = -1;
            int count = this.items.Count;
            for (int i = 0; i < count; i++)
            {
                if (this.items[i] == column)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridColumnCollectionMissing"));
            }
            this.RemoveAt(index);
        }

        public void RemoveAt(int index)
        {
            if (this.isDefault)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultColumnCollectionChanged"));
            }
            DataGridColumnStyle element = (DataGridColumnStyle) this.items[index];
            element.SetDataGridTableInColumn(null, true);
            element.MappingNameChanged -= new EventHandler(this.ColumnStyleMappingNameChanged);
            element.PropertyDescriptorChanged -= new EventHandler(this.ColumnStylePropDescChanged);
            this.items.RemoveAt(index);
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, element));
        }

        internal void ResetDefaultColumnCollection()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].ReleaseHostedControl();
            }
            this.items.Clear();
        }

        public void ResetPropertyDescriptors()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].PropertyDescriptor = null;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.items.CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        int IList.Add(object value)
        {
            return this.Add((DataGridColumnStyle) value);
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            return this.items.Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return this.items.IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object value)
        {
            this.Remove((DataGridColumnStyle) value);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        internal System.Windows.Forms.DataGridTableStyle DataGridTableStyle
        {
            get
            {
                return this.owner;
            }
        }

        public DataGridColumnStyle this[int index]
        {
            get
            {
                return (DataGridColumnStyle) this.items[index];
            }
        }

        public DataGridColumnStyle this[string columnName]
        {
            get
            {
                int count = this.items.Count;
                for (int i = 0; i < count; i++)
                {
                    DataGridColumnStyle style = (DataGridColumnStyle) this.items[i];
                    if (string.Equals(style.MappingName, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        return style;
                    }
                }
                return null;
            }
        }

        public DataGridColumnStyle this[PropertyDescriptor propertyDesciptor]
        {
            get
            {
                int count = this.items.Count;
                for (int i = 0; i < count; i++)
                {
                    DataGridColumnStyle style = (DataGridColumnStyle) this.items[i];
                    if (propertyDesciptor.Equals(style.PropertyDescriptor))
                    {
                        return style;
                    }
                }
                return null;
            }
        }

        protected override ArrayList List
        {
            get
            {
                return this.items;
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.items.Count;
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
                return this.items[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}

