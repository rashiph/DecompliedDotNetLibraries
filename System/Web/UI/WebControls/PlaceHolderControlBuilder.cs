namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    public class PlaceHolderControlBuilder : ControlBuilder
    {
        public override bool AllowWhitespaceLiterals()
        {
            return false;
        }
    }
}

