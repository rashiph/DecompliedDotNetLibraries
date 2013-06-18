namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    public abstract class DesignerDataSourceView
    {
        private string _name;
        private IDataSourceDesigner _owner;

        protected DesignerDataSourceView(IDataSourceDesigner owner, string viewName)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (viewName == null)
            {
                throw new ArgumentNullException("viewName");
            }
            this._owner = owner;
            this._name = viewName;
        }

        public virtual IEnumerable GetDesignTimeData(int minimumRows, out bool isSampleData)
        {
            isSampleData = true;
            return DesignTimeData.GetDesignTimeDataSource(DesignTimeData.CreateDummyDataBoundDataTable(), minimumRows);
        }

        public virtual bool CanDelete
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanInsert
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanPage
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanRetrieveTotalRowCount
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanSort
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanUpdate
        {
            get
            {
                return false;
            }
        }

        public IDataSourceDesigner DataSourceDesigner
        {
            get
            {
                return this._owner;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public virtual IDataSourceViewSchema Schema
        {
            get
            {
                return null;
            }
        }
    }
}

