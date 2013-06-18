namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class StyleCollectionEditor : CollectionEditor
    {
        public StyleCollectionEditor(Type type) : base(type)
        {
        }

        protected override object CreateInstance(Type itemType)
        {
            return Activator.CreateInstance(itemType, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, null, null);
        }
    }
}

