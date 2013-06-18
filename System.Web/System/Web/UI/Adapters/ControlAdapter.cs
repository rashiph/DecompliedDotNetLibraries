namespace System.Web.UI.Adapters
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    public abstract class ControlAdapter
    {
        private HttpBrowserCapabilities _browser;
        internal System.Web.UI.Control _control;

        protected ControlAdapter()
        {
        }

        protected internal virtual void BeginRender(HtmlTextWriter writer)
        {
            writer.BeginRender();
        }

        protected internal virtual void CreateChildControls()
        {
            this.Control.CreateChildControls();
        }

        protected internal virtual void EndRender(HtmlTextWriter writer)
        {
            writer.EndRender();
        }

        protected internal virtual void LoadAdapterControlState(object state)
        {
        }

        protected internal virtual void LoadAdapterViewState(object state)
        {
        }

        protected internal virtual void OnInit(EventArgs e)
        {
            this.Control.OnInit(e);
        }

        protected internal virtual void OnLoad(EventArgs e)
        {
            this.Control.OnLoad(e);
        }

        protected internal virtual void OnPreRender(EventArgs e)
        {
            this.Control.OnPreRender(e);
        }

        protected internal virtual void OnUnload(EventArgs e)
        {
            this.Control.OnUnload(e);
        }

        protected internal virtual void Render(HtmlTextWriter writer)
        {
            if (this._control != null)
            {
                this._control.Render(writer);
            }
        }

        protected virtual void RenderChildren(HtmlTextWriter writer)
        {
            if (this._control != null)
            {
                this._control.RenderChildren(writer);
            }
        }

        protected internal virtual object SaveAdapterControlState()
        {
            return null;
        }

        protected internal virtual object SaveAdapterViewState()
        {
            return null;
        }

        protected HttpBrowserCapabilities Browser
        {
            get
            {
                if (this._browser == null)
                {
                    if (this.Page.RequestInternal != null)
                    {
                        this._browser = this.Page.RequestInternal.Browser;
                    }
                    else
                    {
                        HttpContext current = HttpContext.Current;
                        if ((current != null) && (current.Request != null))
                        {
                            this._browser = current.Request.Browser;
                        }
                    }
                }
                return this._browser;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        protected System.Web.UI.Control Control
        {
            get
            {
                return this._control;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        protected System.Web.UI.Page Page
        {
            get
            {
                if (this.Control != null)
                {
                    return this.Control.Page;
                }
                return null;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected System.Web.UI.Adapters.PageAdapter PageAdapter
        {
            get
            {
                if ((this.Control != null) && (this.Control.Page != null))
                {
                    return this.Control.Page.PageAdapter;
                }
                return null;
            }
        }
    }
}

