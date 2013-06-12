namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [ControlBuilder(typeof(HtmlEmptyTagControlBuilder))]
    public class HtmlLink : HtmlControl
    {
        public HtmlLink() : base("link")
        {
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            writer.WriteBeginTag(this.TagName);
            this.RenderAttributes(writer);
            writer.Write(" />");
        }

        protected override void RenderAttributes(HtmlTextWriter writer)
        {
            if (!string.IsNullOrEmpty(this.Href))
            {
                base.Attributes["href"] = base.ResolveClientUrl(this.Href);
            }
            base.RenderAttributes(writer);
        }

        [UrlProperty, DefaultValue(""), WebCategory("Action"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string Href
        {
            get
            {
                string str = base.Attributes["href"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["href"] = HtmlControl.MapStringAttributeToString(value);
            }
        }
    }
}

