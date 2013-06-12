namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    public class AssociatedControlConverter : ControlIDConverter
    {
        protected override bool FilterControl(Control control)
        {
            return (control is WebControl);
        }
    }
}

