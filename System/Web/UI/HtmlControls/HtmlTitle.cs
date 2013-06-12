namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    public class HtmlTitle : HtmlControl
    {
        private string _text;

        public HtmlTitle() : base("title")
        {
        }

        protected override void AddParsedSubObject(object obj)
        {
            if (obj is LiteralControl)
            {
                this._text = ((LiteralControl) obj).Text;
            }
            else
            {
                base.AddParsedSubObject(obj);
            }
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new ControlCollection(this);
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Title);
            if (this.HasControls() || base.HasRenderDelegate())
            {
                this.RenderChildren(writer);
            }
            else if (this._text != null)
            {
                writer.Write(this._text);
            }
            writer.RenderEndTag();
        }

        [PersistenceMode(PersistenceMode.InnerDefaultProperty), Localizable(true), WebCategory("Appearance"), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string Text
        {
            get
            {
                if (this._text == null)
                {
                    return string.Empty;
                }
                return this._text;
            }
            set
            {
                this._text = value;
            }
        }
    }
}

