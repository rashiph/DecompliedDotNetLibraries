namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class SiteMapDesignerHierarchicalDataSourceView : DesignerHierarchicalDataSourceView
    {
        private SiteMapDataSourceDesigner _owner;

        public SiteMapDesignerHierarchicalDataSourceView(SiteMapDataSourceDesigner owner, string viewPath) : base(owner, viewPath)
        {
            this._owner = owner;
        }

        public override IHierarchicalEnumerable GetDesignTimeData(out bool isSampleData)
        {
            string siteMapProvider = null;
            string startingNodeUrl = null;
            IHierarchicalEnumerable enumerable = null;
            isSampleData = true;
            siteMapProvider = this._owner.SiteMapDataSource.SiteMapProvider;
            startingNodeUrl = this._owner.SiteMapDataSource.StartingNodeUrl;
            this._owner.SiteMapDataSource.Provider = this._owner.DesignTimeSiteMapProvider;
            try
            {
                this._owner.SiteMapDataSource.StartingNodeUrl = null;
                enumerable = ((IHierarchicalDataSource) this._owner.SiteMapDataSource).GetHierarchicalView(base.Path).Select();
                isSampleData = false;
            }
            finally
            {
                this._owner.SiteMapDataSource.StartingNodeUrl = startingNodeUrl;
                this._owner.SiteMapDataSource.SiteMapProvider = siteMapProvider;
            }
            return enumerable;
        }

        public override IDataSourceSchema Schema
        {
            get
            {
                return SiteMapDataSourceDesigner.SiteMapHierarchicalSchema;
            }
        }
    }
}

