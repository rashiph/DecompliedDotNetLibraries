namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class HyperLinkDataBindingHandler : DataBindingHandler
    {
        public override void DataBindControl(IDesignerHost designerHost, Control control)
        {
            DataBindingCollection dataBindings = ((IDataBindingsAccessor) control).DataBindings;
            DataBinding binding = dataBindings["Text"];
            DataBinding binding2 = dataBindings["NavigateUrl"];
            if ((binding != null) || (binding2 != null))
            {
                HyperLink link = (HyperLink) control;
                if (binding != null)
                {
                    link.Text = System.Design.SR.GetString("Sample_Databound_Text");
                }
                if (binding2 != null)
                {
                    link.NavigateUrl = "url";
                }
            }
        }
    }
}

