namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ListItemsCollectionEditor : CollectionEditor
    {
        public ListItemsCollectionEditor(Type type) : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.ComponentModel.CollectionEditor";
            }
        }
    }
}

