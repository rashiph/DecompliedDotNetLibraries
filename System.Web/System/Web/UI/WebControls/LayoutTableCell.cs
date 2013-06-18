namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    internal sealed class LayoutTableCell : TableCell
    {
        protected internal override void AddedControl(Control control, int index)
        {
            if (control.Page == null)
            {
                control.Page = this.Page;
            }
        }

        protected internal override void RemovedControl(Control control)
        {
        }
    }
}

