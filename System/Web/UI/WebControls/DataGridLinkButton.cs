namespace System.Web.UI.WebControls
{
    using System;
    using System.Drawing;
    using System.Web.UI;

    [SupportsEventValidation]
    internal sealed class DataGridLinkButton : LinkButton
    {
        internal DataGridLinkButton()
        {
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            this.SetForeColor();
            base.Render(writer);
        }

        private void SetForeColor()
        {
            if (!base.ControlStyle.IsSet(4))
            {
                Control parent = this;
                for (int i = 0; i < 3; i++)
                {
                    parent = parent.Parent;
                    Color foreColor = ((WebControl) parent).ForeColor;
                    if (foreColor != Color.Empty)
                    {
                        this.ForeColor = foreColor;
                        return;
                    }
                }
            }
        }
    }
}

