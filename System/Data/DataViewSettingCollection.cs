namespace System.Data
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;

    [Editor("Microsoft.VSDesigner.Data.Design.DataViewSettingsCollectionEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class DataViewSettingCollection : ICollection, IEnumerable
    {
        private readonly DataViewManager dataViewManager;
        private readonly Hashtable list = new Hashtable();

        internal DataViewSettingCollection(DataViewManager dataViewManager)
        {
            if (dataViewManager == null)
            {
                throw ExceptionBuilder.ArgumentNull("dataViewManager");
            }
            this.dataViewManager = dataViewManager;
        }

        public void CopyTo(Array ar, int index)
        {
            IEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ar.SetValue(enumerator.Current, index++);
            }
        }

        public void CopyTo(DataViewSetting[] ar, int index)
        {
            IEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ar.SetValue(enumerator.Current, index++);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new DataViewSettingsEnumerator(this.dataViewManager);
        }

        private DataTable GetTable(int index)
        {
            DataTable table = null;
            DataSet dataSet = this.dataViewManager.DataSet;
            if (dataSet != null)
            {
                table = dataSet.Tables[index];
            }
            return table;
        }

        private DataTable GetTable(string tableName)
        {
            DataTable table = null;
            DataSet dataSet = this.dataViewManager.DataSet;
            if (dataSet != null)
            {
                table = dataSet.Tables[tableName];
            }
            return table;
        }

        internal void Remove(DataTable table)
        {
            this.list.Remove(table);
        }

        [Browsable(false)]
        public virtual int Count
        {
            get
            {
                DataSet dataSet = this.dataViewManager.DataSet;
                if (dataSet != null)
                {
                    return dataSet.Tables.Count;
                }
                return 0;
            }
        }

        [Browsable(false)]
        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        [Browsable(false)]
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual DataViewSetting this[DataTable table]
        {
            get
            {
                if (table == null)
                {
                    throw ExceptionBuilder.ArgumentNull("table");
                }
                DataViewSetting setting = (DataViewSetting) this.list[table];
                if (setting == null)
                {
                    setting = new DataViewSetting();
                    this[table] = setting;
                }
                return setting;
            }
            set
            {
                if (table == null)
                {
                    throw ExceptionBuilder.ArgumentNull("table");
                }
                value.SetDataViewManager(this.dataViewManager);
                value.SetDataTable(table);
                this.list[table] = value;
            }
        }

        public virtual DataViewSetting this[string tableName]
        {
            get
            {
                DataTable table = this.GetTable(tableName);
                if (table != null)
                {
                    return this[table];
                }
                return null;
            }
        }

        public virtual DataViewSetting this[int index]
        {
            get
            {
                DataTable table = this.GetTable(index);
                if (table != null)
                {
                    return this[table];
                }
                return null;
            }
            set
            {
                DataTable table = this.GetTable(index);
                if (table != null)
                {
                    this[table] = value;
                }
            }
        }

        [Browsable(false)]
        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        private sealed class DataViewSettingsEnumerator : IEnumerator
        {
            private DataViewSettingCollection dataViewSettings;
            private IEnumerator tableEnumerator;

            public DataViewSettingsEnumerator(DataViewManager dvm)
            {
                if (dvm.DataSet != null)
                {
                    this.dataViewSettings = dvm.DataViewSettings;
                    this.tableEnumerator = dvm.DataSet.Tables.GetEnumerator();
                }
                else
                {
                    this.dataViewSettings = null;
                    this.tableEnumerator = DataSet.zeroTables.GetEnumerator();
                }
            }

            public bool MoveNext()
            {
                return this.tableEnumerator.MoveNext();
            }

            public void Reset()
            {
                this.tableEnumerator.Reset();
            }

            public object Current
            {
                get
                {
                    return this.dataViewSettings[(DataTable) this.tableEnumerator.Current];
                }
            }
        }
    }
}

