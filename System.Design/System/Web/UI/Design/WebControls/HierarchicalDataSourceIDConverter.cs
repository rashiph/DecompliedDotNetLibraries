namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;

    public class HierarchicalDataSourceIDConverter : DataSourceIDConverter
    {
        protected override bool IsValidDataSource(IComponent component)
        {
            return (component is IHierarchicalDataSource);
        }
    }
}

