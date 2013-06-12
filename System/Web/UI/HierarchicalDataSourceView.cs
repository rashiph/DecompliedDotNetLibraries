namespace System.Web.UI
{
    using System;

    public abstract class HierarchicalDataSourceView
    {
        protected HierarchicalDataSourceView()
        {
        }

        public abstract IHierarchicalEnumerable Select();
    }
}

