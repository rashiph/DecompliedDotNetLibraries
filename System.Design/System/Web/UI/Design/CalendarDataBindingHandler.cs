namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class CalendarDataBindingHandler : DataBindingHandler
    {
        public override void DataBindControl(IDesignerHost designerHost, Control control)
        {
            Calendar calendar = (Calendar) control;
            if (calendar.DataBindings["SelectedDate"] != null)
            {
                calendar.SelectedDate = DateTime.Today;
            }
        }
    }
}

