namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    internal sealed class LabelLiteral : Literal
    {
        internal Control _for;
        internal bool _renderAsLabel;

        internal LabelLiteral(Control forControl)
        {
            this._for = forControl;
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this.RenderAsLabel)
            {
                writer.Write("<asp:label runat=\"server\" AssociatedControlID=\"");
                writer.Write(this._for.ID);
                writer.Write("\" ID=\"");
                writer.Write(this._for.ID);
                writer.Write("Label\">");
                writer.Write(base.Text);
                writer.Write("</asp:label>");
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.For, this._for.ClientID);
                writer.RenderBeginTag(HtmlTextWriterTag.Label);
                base.Render(writer);
                writer.RenderEndTag();
            }
        }

        internal bool RenderAsLabel
        {
            get
            {
                return this._renderAsLabel;
            }
            set
            {
                this._renderAsLabel = value;
            }
        }
    }
}

