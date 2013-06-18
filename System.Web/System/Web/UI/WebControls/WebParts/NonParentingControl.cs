namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Web.UI;

    internal sealed class NonParentingControl : Control
    {
        protected internal override void AddedControl(Control control, int index)
        {
        }

        protected internal override void RemovedControl(Control control)
        {
        }
    }
}

