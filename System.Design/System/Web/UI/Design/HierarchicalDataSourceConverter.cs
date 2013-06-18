namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Web.UI;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class HierarchicalDataSourceConverter : DataSourceConverter
    {
        protected override bool IsValidDataSource(IComponent component)
        {
            Control control = component as Control;
            if (control == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(control.ID))
            {
                return false;
            }
            return (component is IHierarchicalEnumerable);
        }
    }
}

