namespace System.Data
{
    using System;
    using System.ComponentModel;

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataViewSetting
    {
        private bool applyDefaultSort;
        private System.Data.DataViewManager dataViewManager;
        private string rowFilter;
        private DataViewRowState rowStateFilter;
        private string sort;
        private DataTable table;

        internal DataViewSetting()
        {
            this.sort = "";
            this.rowFilter = "";
            this.rowStateFilter = DataViewRowState.CurrentRows;
        }

        internal DataViewSetting(string sort, string rowFilter, DataViewRowState rowStateFilter)
        {
            this.sort = "";
            this.rowFilter = "";
            this.rowStateFilter = DataViewRowState.CurrentRows;
            this.sort = sort;
            this.rowFilter = rowFilter;
            this.rowStateFilter = rowStateFilter;
        }

        internal void SetDataTable(DataTable table)
        {
            if (this.table != table)
            {
                DataTable table1 = this.table;
                this.table = table;
            }
        }

        internal void SetDataViewManager(System.Data.DataViewManager dataViewManager)
        {
            if (this.dataViewManager != dataViewManager)
            {
                System.Data.DataViewManager manager1 = this.dataViewManager;
                this.dataViewManager = dataViewManager;
            }
        }

        public bool ApplyDefaultSort
        {
            get
            {
                return this.applyDefaultSort;
            }
            set
            {
                if (this.applyDefaultSort != value)
                {
                    this.applyDefaultSort = value;
                }
            }
        }

        [Browsable(false)]
        public System.Data.DataViewManager DataViewManager
        {
            get
            {
                return this.dataViewManager;
            }
        }

        public string RowFilter
        {
            get
            {
                return this.rowFilter;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if (this.rowFilter != value)
                {
                    this.rowFilter = value;
                }
            }
        }

        public DataViewRowState RowStateFilter
        {
            get
            {
                return this.rowStateFilter;
            }
            set
            {
                if (this.rowStateFilter != value)
                {
                    this.rowStateFilter = value;
                }
            }
        }

        public string Sort
        {
            get
            {
                return this.sort;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if (this.sort != value)
                {
                    this.sort = value;
                }
            }
        }

        [Browsable(false)]
        public DataTable Table
        {
            get
            {
                return this.table;
            }
        }
    }
}

