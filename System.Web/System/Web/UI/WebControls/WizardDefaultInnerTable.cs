namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    [SupportsEventValidation]
    internal class WizardDefaultInnerTable : Table
    {
        internal WizardDefaultInnerTable()
        {
            base.PreventAutoID();
            this.CellPadding = 0;
            this.CellSpacing = 0;
        }
    }
}

