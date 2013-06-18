namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    using System.Web.UI;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public abstract class DataBindingHandler
    {
        protected DataBindingHandler()
        {
        }

        public abstract void DataBindControl(IDesignerHost designerHost, Control control);
    }
}

