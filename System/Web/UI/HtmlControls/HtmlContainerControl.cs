namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    public abstract class HtmlContainerControl : HtmlControl
    {
        protected HtmlContainerControl() : this("span")
        {
        }

        public HtmlContainerControl(string tag) : base(tag)
        {
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new ControlCollection(this);
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                base.LoadViewState(savedState);
                string text = (string) this.ViewState["innerhtml"];
                if (text != null)
                {
                    this.Controls.Clear();
                    this.Controls.Add(new LiteralControl(text));
                }
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            this.RenderBeginTag(writer);
            this.RenderChildren(writer);
            this.RenderEndTag(writer);
        }

        protected override void RenderAttributes(HtmlTextWriter writer)
        {
            this.ViewState.Remove("innerhtml");
            base.RenderAttributes(writer);
        }

        protected virtual void RenderEndTag(HtmlTextWriter writer)
        {
            writer.WriteEndTag(this.TagName);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), HtmlControlPersistable(false)]
        public virtual string InnerHtml
        {
            get
            {
                if (base.IsLiteralContent())
                {
                    return ((LiteralControl) this.Controls[0]).Text;
                }
                if ((this.HasControls() && (this.Controls.Count == 1)) && (this.Controls[0] is DataBoundLiteralControl))
                {
                    return ((DataBoundLiteralControl) this.Controls[0]).Text;
                }
                if (this.Controls.Count != 0)
                {
                    throw new HttpException(System.Web.SR.GetString("Inner_Content_not_literal", new object[] { this.ID }));
                }
                return string.Empty;
            }
            set
            {
                this.Controls.Clear();
                this.Controls.Add(new LiteralControl(value));
                this.ViewState["innerhtml"] = value;
            }
        }

        [HtmlControlPersistable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual string InnerText
        {
            get
            {
                return HttpUtility.HtmlDecode(this.InnerHtml);
            }
            set
            {
                this.InnerHtml = HttpUtility.HtmlEncode(value);
            }
        }
    }
}

