namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [Designer("System.Web.UI.Design.WebControls.WebParts.PageCatalogPartDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public sealed class PageCatalogPart : CatalogPart
    {
        private WebPartDescriptionCollection _availableWebPartDescriptions;
        private static readonly WebPartDescriptionCollection DesignModeAvailableWebParts = new WebPartDescriptionCollection(new WebPartDescription[] { new WebPartDescription("webpart1", string.Format(CultureInfo.CurrentCulture, System.Web.SR.GetString("CatalogPart_SampleWebPartTitle"), new object[] { "1" }), null, null), new WebPartDescription("webpart2", string.Format(CultureInfo.CurrentCulture, System.Web.SR.GetString("CatalogPart_SampleWebPartTitle"), new object[] { "2" }), null, null), new WebPartDescription("webpart3", string.Format(CultureInfo.CurrentCulture, System.Web.SR.GetString("CatalogPart_SampleWebPartTitle"), new object[] { "3" }), null, null) });

        public override WebPartDescriptionCollection GetAvailableWebPartDescriptions()
        {
            if (base.DesignMode)
            {
                return DesignModeAvailableWebParts;
            }
            if (this._availableWebPartDescriptions == null)
            {
                WebPartCollection parts;
                if (base.WebPartManager != null)
                {
                    WebPartCollection closedWebParts = this.GetClosedWebParts();
                    if (closedWebParts != null)
                    {
                        parts = closedWebParts;
                    }
                    else
                    {
                        parts = new WebPartCollection();
                    }
                }
                else
                {
                    parts = new WebPartCollection();
                }
                ArrayList webPartDescriptions = new ArrayList();
                foreach (WebPart part in parts)
                {
                    if (!(part is UnauthorizedWebPart))
                    {
                        WebPartDescription description = new WebPartDescription(part);
                        webPartDescriptions.Add(description);
                    }
                }
                this._availableWebPartDescriptions = new WebPartDescriptionCollection(webPartDescriptions);
            }
            return this._availableWebPartDescriptions;
        }

        private WebPartCollection GetClosedWebParts()
        {
            ArrayList webParts = new ArrayList();
            WebPartCollection parts = base.WebPartManager.WebParts;
            if (parts != null)
            {
                foreach (WebPart part in parts)
                {
                    if (part.IsClosed)
                    {
                        webParts.Add(part);
                    }
                }
            }
            return new WebPartCollection(webParts);
        }

        public override WebPart GetWebPart(WebPartDescription description)
        {
            if (description == null)
            {
                throw new ArgumentNullException("description");
            }
            if (!this.GetAvailableWebPartDescriptions().Contains(description))
            {
                throw new ArgumentException(System.Web.SR.GetString("CatalogPart_UnknownDescription"), "description");
            }
            return description.WebPart;
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (base.WebPartManager != null)
            {
                base.WebPartManager.WebPartAdded += new WebPartEventHandler(this.OnWebPartsChanged);
                base.WebPartManager.WebPartClosed += new WebPartEventHandler(this.OnWebPartsChanged);
                base.WebPartManager.WebPartDeleted += new WebPartEventHandler(this.OnWebPartsChanged);
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this._availableWebPartDescriptions = null;
        }

        private void OnWebPartsChanged(object sender, WebPartEventArgs e)
        {
            this._availableWebPartDescriptions = null;
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
        }

        [Themeable(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string AccessKey
        {
            get
            {
                return base.AccessKey;
            }
            set
            {
                base.AccessKey = value;
            }
        }

        [Browsable(false), Themeable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Themeable(false), Browsable(false)]
        public override string BackImageUrl
        {
            get
            {
                return base.BackImageUrl;
            }
            set
            {
                base.BackImageUrl = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
            }
        }

        [Themeable(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override System.Web.UI.WebControls.BorderStyle BorderStyle
        {
            get
            {
                return base.BorderStyle;
            }
            set
            {
                base.BorderStyle = value;
            }
        }

        [Themeable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override Unit BorderWidth
        {
            get
            {
                return base.BorderWidth;
            }
            set
            {
                base.BorderWidth = value;
            }
        }

        [Themeable(false), CssClassProperty, EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override string CssClass
        {
            get
            {
                return base.CssClass;
            }
            set
            {
                base.CssClass = value;
            }
        }

        [Themeable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override string DefaultButton
        {
            get
            {
                return base.DefaultButton;
            }
            set
            {
                base.DefaultButton = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), Themeable(false)]
        public override ContentDirection Direction
        {
            get
            {
                return base.Direction;
            }
            set
            {
                base.Direction = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
            }
        }

        [Themeable(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DefaultValue(false)]
        public override bool EnableTheming
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("NoThemingSupport", new object[] { base.GetType().Name }));
            }
        }

        [Browsable(false), Themeable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override FontInfo Font
        {
            get
            {
                return base.Font;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [Themeable(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string GroupingText
        {
            get
            {
                return base.GroupingText;
            }
            set
            {
                base.GroupingText = value;
            }
        }

        [Themeable(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override Unit Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
            }
        }

        [Themeable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override System.Web.UI.WebControls.HorizontalAlign HorizontalAlign
        {
            get
            {
                return base.HorizontalAlign;
            }
            set
            {
                base.HorizontalAlign = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), Themeable(false)]
        public override System.Web.UI.WebControls.ScrollBars ScrollBars
        {
            get
            {
                return base.ScrollBars;
            }
            set
            {
                base.ScrollBars = value;
            }
        }

        [Browsable(false), DefaultValue(""), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override string SkinID
        {
            get
            {
                return string.Empty;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("NoThemingSupport", new object[] { base.GetType().Name }));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), Themeable(false)]
        public override short TabIndex
        {
            get
            {
                return base.TabIndex;
            }
            set
            {
                base.TabIndex = value;
            }
        }

        [WebSysDefaultValue("PageCatalogPart_PartTitle")]
        public override string Title
        {
            get
            {
                string str = (string) this.ViewState["Title"];
                if (str == null)
                {
                    return System.Web.SR.GetString("PageCatalogPart_PartTitle");
                }
                return str;
            }
            set
            {
                this.ViewState["Title"] = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), Themeable(false)]
        public override string ToolTip
        {
            get
            {
                return base.ToolTip;
            }
            set
            {
                base.ToolTip = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), Themeable(false)]
        public override Unit Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override bool Wrap
        {
            get
            {
                return base.Wrap;
            }
            set
            {
                base.Wrap = value;
            }
        }
    }
}

