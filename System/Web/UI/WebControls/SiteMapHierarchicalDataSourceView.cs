namespace System.Web.UI.WebControls
{
    using System;
    using System.Web;
    using System.Web.UI;

    public class SiteMapHierarchicalDataSourceView : HierarchicalDataSourceView
    {
        private SiteMapNodeCollection _collection;

        public SiteMapHierarchicalDataSourceView(SiteMapNode node)
        {
            this._collection = new SiteMapNodeCollection(node);
        }

        public SiteMapHierarchicalDataSourceView(SiteMapNodeCollection collection)
        {
            this._collection = collection;
        }

        public override IHierarchicalEnumerable Select()
        {
            return this._collection;
        }
    }
}

