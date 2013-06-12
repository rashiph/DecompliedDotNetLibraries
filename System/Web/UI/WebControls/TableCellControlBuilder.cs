namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    public class TableCellControlBuilder : ControlBuilder
    {
        public override bool AllowWhitespaceLiterals()
        {
            return false;
        }
    }
}

