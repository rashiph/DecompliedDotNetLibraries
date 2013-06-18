namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    using System.Web.UI.Design;

    [SupportsPreviewControl(true), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class LabelDesigner : TextControlDesigner
    {
        public override void OnComponentChanged(object sender, ComponentChangedEventArgs ce)
        {
            base.OnComponentChanged(sender, new ComponentChangedEventArgs(ce.Component, null, null, null));
        }
    }
}

