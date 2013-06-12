namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;

    [ListBindable(false)]
    public class GridTableStylesCollection : BaseCollection, IList, ICollection, IEnumerable
    {
        private ArrayList items = new ArrayList();
        private DataGrid owner;

        public event CollectionChangeEventHandler CollectionChanged;

        internal GridTableStylesCollection(DataGrid grid)
        {
            this.owner = grid;
        }

        public virtual int Add(DataGridTableStyle table)
        {
            if ((this.owner != null) && (this.owner.MinimumRowHeaderWidth() > table.RowHeaderWidth))
            {
                table.RowHeaderWidth = this.owner.MinimumRowHeaderWidth();
            }
            if ((table.DataGrid != this.owner) && (table.DataGrid != null))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridTableStyleCollectionAddedParentedTableStyle"), "table");
            }
            table.DataGrid = this.owner;
            this.CheckForMappingNameDuplicates(table);
            table.MappingNameChanged += new EventHandler(this.TableStyleMappingNameChanged);
            int num = this.items.Add(table);
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, table));
            return num;
        }

        public virtual void AddRange(DataGridTableStyle[] tables)
        {
            if (tables == null)
            {
                throw new ArgumentNullException("tables");
            }
            foreach (DataGridTableStyle style in tables)
            {
                style.DataGrid = this.owner;
                style.MappingNameChanged += new EventHandler(this.TableStyleMappingNameChanged);
                this.items.Add(style);
            }
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        internal void CheckForMappingNameDuplicates(DataGridTableStyle table)
        {
            if (!string.IsNullOrEmpty(table.MappingName))
            {
                for (int i = 0; i < this.items.Count; i++)
                {
                    if (((DataGridTableStyle) this.items[i]).MappingName.Equals(table.MappingName) && (table != this.items[i]))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridTableStyleDuplicateMappingName"), "table");
                    }
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < this.items.Count; i++)
            {
                DataGridTableStyle style = (DataGridTableStyle) this.items[i];
                style.MappingNameChanged -= new EventHandler(this.TableStyleMappingNameChanged);
            }
            this.items.Clear();
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        public bool Contains(string name)
        {
            int count = this.items.Count;
            for (int i = 0; i < count; i++)
            {
                DataGridTableStyle style = (DataGridTableStyle) this.items[i];
                if (string.Compare(style.MappingName, name, true, CultureInfo.InvariantCulture) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(DataGridTableStyle table)
        {
            return (this.items.IndexOf(table) != -1);
        }

        protected void OnCollectionChanged(CollectionChangeEventArgs e)
        {
            if (this.onCollectionChanged != null)
            {
                this.onCollectionChanged(this, e);
            }
            DataGrid owner = this.owner;
            if (owner != null)
            {
                owner.checkHierarchy = true;
            }
        }

        public void Remove(DataGridTableStyle table)
        {
            int index = -1;
            int count = this.items.Count;
            for (int i = 0; i < count; i++)
            {
                if (this.items[i] == table)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridTableCollectionMissingTable"), "table");
            }
            this.RemoveAt(index);
        }

        public void RemoveAt(int index)
        {
            DataGridTableStyle element = (DataGridTableStyle) this.items[index];
            element.MappingNameChanged -= new EventHandler(this.TableStyleMappingNameChanged);
            this.items.RemoveAt(index);
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, element));
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
            return this.Add((DataGridTableStyle) value);
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
            this.Remove((DataGridTableStyle) value);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        private void TableStyleMappingNameChanged(object sender, EventArgs pcea)
        {
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        public DataGridTableStyle this[int index]
        {
            get
            {
                return (DataGridTableStyle) this.items[index];
            }
        }

        public DataGridTableStyle this[string tableName]
        {
            get
            {
                if (tableName == null)
                {
                    throw new ArgumentNullException("tableName");
                }
                int count = this.items.Count;
                for (int i = 0; i < count; i++)
                {
                    DataGridTableStyle style = (DataGridTableStyle) this.items[i];
                    if (string.Equals(style.MappingName, tableName, StringComparison.OrdinalIgnoreCase))
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

