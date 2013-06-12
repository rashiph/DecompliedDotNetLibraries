namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    internal sealed class ReadOnlyHierarchicalDataSourceView : HierarchicalDataSourceView
    {
        private IHierarchicalEnumerable _dataSource;

        public ReadOnlyHierarchicalDataSourceView(IHierarchicalEnumerable dataSource)
        {
            this._dataSource = dataSource;
        }

        public override IHierarchicalEnumerable Select()
        {
            return this._dataSource;
        }
    }
}

