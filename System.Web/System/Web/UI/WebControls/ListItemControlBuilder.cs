namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    public class ListItemControlBuilder : ControlBuilder
    {
        public override bool AllowWhitespaceLiterals()
        {
            return false;
        }

        public override bool HtmlDecodeLiterals()
        {
            return true;
        }
    }
}

