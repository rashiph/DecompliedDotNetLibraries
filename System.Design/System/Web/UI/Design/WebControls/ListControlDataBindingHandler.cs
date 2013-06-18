namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ListControlDataBindingHandler : DataBindingHandler
    {
        public override void DataBindControl(IDesignerHost designerHost, Control control)
        {
            if (((IDataBindingsAccessor) control).DataBindings["DataSource"] != null)
            {
                ListControl control2 = (ListControl) control;
                control2.Items.Clear();
                control2.Items.Add(System.Design.SR.GetString("Sample_Databound_Text"));
            }
        }
    }
}

