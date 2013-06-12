namespace System.Web.UI.WebControls.Adapters
{
    using System;
    using System.Web.UI;
    using System.Web.UI.Adapters;
    using System.Web.UI.WebControls;

    public class WebControlAdapter : ControlAdapter
    {
        protected internal override void Render(HtmlTextWriter writer)
        {
            this.RenderBeginTag(writer);
            this.RenderContents(writer);
            this.RenderEndTag(writer);
        }

        protected virtual void RenderBeginTag(HtmlTextWriter writer)
        {
            this.Control.RenderBeginTag(writer);
        }

        protected virtual void RenderContents(HtmlTextWriter writer)
        {
            this.Control.RenderContents(writer);
        }

        protected virtual void RenderEndTag(HtmlTextWriter writer)
        {
            this.Control.RenderEndTag(writer);
        }

        protected WebControl Control
        {
            get
            {
                return (WebControl) base.Control;
            }
        }

        protected bool IsEnabled
        {
            get
            {
                return this.Control.IsEnabled;
            }
        }
    }
}

