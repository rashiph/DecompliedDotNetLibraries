namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class SiteMapDesignerDataSourceView : DesignerDataSourceView
    {
        private SiteMapDataSourceDesigner _owner;
        private SiteMapDataSource _siteMapDataSource;
        private static readonly SiteMapDataSourceDesigner.SiteMapDataSourceViewSchema _siteMapViewSchema = new SiteMapDataSourceDesigner.SiteMapDataSourceViewSchema();

        public SiteMapDesignerDataSourceView(SiteMapDataSourceDesigner owner, string viewName) : base(owner, viewName)
        {
            this._owner = owner;
            this._siteMapDataSource = (SiteMapDataSource) this._owner.Component;
        }

        public override IEnumerable GetDesignTimeData(int minimumRows, out bool isSampleData)
        {
            string siteMapProvider = null;
            string startingNodeUrl = null;
            SiteMapNodeCollection nodes = null;
            siteMapProvider = this._siteMapDataSource.SiteMapProvider;
            startingNodeUrl = this._siteMapDataSource.StartingNodeUrl;
            this._siteMapDataSource.Provider = this._owner.DesignTimeSiteMapProvider;
            try
            {
                this._siteMapDataSource.StartingNodeUrl = null;
                nodes = ((SiteMapDataSourceView) ((IDataSource) this._siteMapDataSource).GetView(base.Name)).Select(DataSourceSelectArguments.Empty) as SiteMapNodeCollection;
                isSampleData = false;
            }
            finally
            {
                this._siteMapDataSource.StartingNodeUrl = startingNodeUrl;
                this._siteMapDataSource.SiteMapProvider = siteMapProvider;
            }
            if ((nodes != null) && (nodes.Count == 0))
            {
                isSampleData = true;
                return DesignTimeData.GetDesignTimeDataSource(DesignTimeData.CreateDummyDataBoundDataTable(), minimumRows);
            }
            return nodes;
        }

        public override IDataSourceViewSchema Schema
        {
            get
            {
                return _siteMapViewSchema;
            }
        }
    }
}

