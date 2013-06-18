namespace System.Web.UI.Design
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web.UI;

    public abstract class DesignerHierarchicalDataSourceView
    {
        private IHierarchicalDataSourceDesigner _owner;
        private string _path;

        protected DesignerHierarchicalDataSourceView(IHierarchicalDataSourceDesigner owner, string viewPath)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (viewPath == null)
            {
                throw new ArgumentNullException("viewPath");
            }
            this._owner = owner;
            this._path = viewPath;
        }

        public virtual IHierarchicalEnumerable GetDesignTimeData(out bool isSampleData)
        {
            isSampleData = true;
            return null;
        }

        public IHierarchicalDataSourceDesigner DataSourceDesigner
        {
            get
            {
                return this._owner;
            }
        }

        public string Path
        {
            get
            {
                return this._path;
            }
        }

        public virtual IDataSourceSchema Schema
        {
            get
            {
                return null;
            }
        }
    }
}

