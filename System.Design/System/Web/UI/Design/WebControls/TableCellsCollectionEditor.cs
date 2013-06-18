namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class TableCellsCollectionEditor : CollectionEditor
    {
        public TableCellsCollectionEditor(Type type) : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override object CreateInstance(Type itemType)
        {
            return Activator.CreateInstance(itemType, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, null, null);
        }
    }
}

