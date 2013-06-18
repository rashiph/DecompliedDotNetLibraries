namespace System.Web.UI.WebControls.Adapters
{
    using System;
    using System.Web.UI;

    public class HideDisabledControlAdapter : WebControlAdapter
    {
        protected internal override void Render(HtmlTextWriter writer)
        {
            if (base.Control.Enabled)
            {
                base.Control.Render(writer);
            }
        }
    }
}

