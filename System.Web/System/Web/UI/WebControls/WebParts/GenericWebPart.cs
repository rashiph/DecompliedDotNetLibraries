namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [ToolboxItem(false)]
    public class GenericWebPart : WebPart
    {
        private Control _childControl;
        private IWebPart _childIWebPart;
        private string _subtitle;
        internal const string IDPrefix = "gwp";

        protected internal GenericWebPart(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (control is WebPart)
            {
                throw new ArgumentException(System.Web.SR.GetString("GenericWebPart_CannotWrapWebPart"), "control");
            }
            if (control is BasePartialCachingControl)
            {
                throw new ArgumentException(System.Web.SR.GetString("GenericWebPart_CannotWrapOutputCachedControl"), "control");
            }
            if (string.IsNullOrEmpty(control.ID))
            {
                throw new ArgumentException(System.Web.SR.GetString("GenericWebPart_NoID", new object[] { control.GetType().FullName }));
            }
            this.ID = "gwp" + control.ID;
            this._childControl = control;
            this._childIWebPart = this._childControl as IWebPart;
            this.CopyChildAttributes();
        }

        private void CopyChildAttributes()
        {
            IAttributeAccessor childControl = this.ChildControl as IAttributeAccessor;
            if (childControl != null)
            {
                base.AuthorizationFilter = childControl.GetAttribute("AuthorizationFilter");
                base.CatalogIconImageUrl = childControl.GetAttribute("CatalogIconImageUrl");
                base.Description = childControl.GetAttribute("Description");
                string attribute = childControl.GetAttribute("ExportMode");
                if (attribute != null)
                {
                    base.ExportMode = (WebPartExportMode) Util.GetEnumAttribute("ExportMode", attribute, typeof(WebPartExportMode));
                }
                this._subtitle = childControl.GetAttribute("Subtitle");
                base.Title = childControl.GetAttribute("Title");
                base.TitleIconImageUrl = childControl.GetAttribute("TitleIconImageUrl");
                base.TitleUrl = childControl.GetAttribute("TitleUrl");
            }
            WebControl control = this.ChildControl as WebControl;
            if (control != null)
            {
                control.Attributes.Remove("AuthorizationFilter");
                control.Attributes.Remove("CatalogIconImageUrl");
                control.Attributes.Remove("Description");
                control.Attributes.Remove("ExportMode");
                control.Attributes.Remove("Subtitle");
                control.Attributes.Remove("Title");
                control.Attributes.Remove("TitleIconImageUrl");
                control.Attributes.Remove("TitleUrl");
            }
            else if (childControl != null)
            {
                childControl.SetAttribute("AuthorizationFilter", null);
                childControl.SetAttribute("CatalogIconImageUrl", null);
                childControl.SetAttribute("Description", null);
                childControl.SetAttribute("ExportMode", null);
                childControl.SetAttribute("Subtitle", null);
                childControl.SetAttribute("Title", null);
                childControl.SetAttribute("TitleIconImageUrl", null);
                childControl.SetAttribute("TitleUrl", null);
            }
        }

        protected internal override void CreateChildControls()
        {
            ((GenericWebPartControlCollection) this.Controls).AddGenericControl(this.ChildControl);
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new GenericWebPartControlCollection(this);
        }

        public override EditorPartCollection CreateEditorParts()
        {
            IWebEditable childControl = this.ChildControl as IWebEditable;
            if (childControl != null)
            {
                return new EditorPartCollection(base.CreateEditorParts(), childControl.CreateEditorParts());
            }
            return base.CreateEditorParts();
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (base.DesignMode)
            {
                this.EnsureChildControls();
            }
            this.RenderContents(writer);
        }

        public override string CatalogIconImageUrl
        {
            get
            {
                if (this._childIWebPart != null)
                {
                    return this._childIWebPart.CatalogIconImageUrl;
                }
                return base.CatalogIconImageUrl;
            }
            set
            {
                if (this._childIWebPart != null)
                {
                    this._childIWebPart.CatalogIconImageUrl = value;
                }
                else
                {
                    base.CatalogIconImageUrl = value;
                }
            }
        }

        public Control ChildControl
        {
            get
            {
                return this._childControl;
            }
        }

        public override string Description
        {
            get
            {
                if (this._childIWebPart != null)
                {
                    return this._childIWebPart.Description;
                }
                return base.Description;
            }
            set
            {
                if (this._childIWebPart != null)
                {
                    this._childIWebPart.Description = value;
                }
                else
                {
                    base.Description = value;
                }
            }
        }

        public override Unit Height
        {
            get
            {
                WebControl childControl = this.ChildControl as WebControl;
                if (childControl != null)
                {
                    return childControl.Height;
                }
                return base.Height;
            }
            set
            {
                WebControl childControl = this.ChildControl as WebControl;
                if (childControl != null)
                {
                    childControl.Height = value;
                }
                else
                {
                    base.Height = value;
                }
            }
        }

        public sealed override string ID
        {
            get
            {
                return base.ID;
            }
            set
            {
                base.ID = value;
            }
        }

        public override string Subtitle
        {
            get
            {
                if (this._childIWebPart != null)
                {
                    return this._childIWebPart.Subtitle;
                }
                if (this._subtitle == null)
                {
                    return string.Empty;
                }
                return this._subtitle;
            }
        }

        public override string Title
        {
            get
            {
                if (this._childIWebPart != null)
                {
                    return this._childIWebPart.Title;
                }
                return base.Title;
            }
            set
            {
                if (this._childIWebPart != null)
                {
                    this._childIWebPart.Title = value;
                }
                else
                {
                    base.Title = value;
                }
            }
        }

        public override string TitleIconImageUrl
        {
            get
            {
                if (this._childIWebPart != null)
                {
                    return this._childIWebPart.TitleIconImageUrl;
                }
                return base.TitleIconImageUrl;
            }
            set
            {
                if (this._childIWebPart != null)
                {
                    this._childIWebPart.TitleIconImageUrl = value;
                }
                else
                {
                    base.TitleIconImageUrl = value;
                }
            }
        }

        public override string TitleUrl
        {
            get
            {
                if (this._childIWebPart != null)
                {
                    return this._childIWebPart.TitleUrl;
                }
                return base.TitleUrl;
            }
            set
            {
                if (this._childIWebPart != null)
                {
                    this._childIWebPart.TitleUrl = value;
                }
                else
                {
                    base.TitleUrl = value;
                }
            }
        }

        public override WebPartVerbCollection Verbs
        {
            get
            {
                if (this.ChildControl != null)
                {
                    IWebActionable childControl = this.ChildControl as IWebActionable;
                    if (childControl != null)
                    {
                        return new WebPartVerbCollection(base.Verbs, childControl.Verbs);
                    }
                }
                return base.Verbs;
            }
        }

        public override object WebBrowsableObject
        {
            get
            {
                IWebEditable childControl = this.ChildControl as IWebEditable;
                if (childControl != null)
                {
                    return childControl.WebBrowsableObject;
                }
                return this.ChildControl;
            }
        }

        public override Unit Width
        {
            get
            {
                WebControl childControl = this.ChildControl as WebControl;
                if (childControl != null)
                {
                    return childControl.Width;
                }
                return base.Width;
            }
            set
            {
                WebControl childControl = this.ChildControl as WebControl;
                if (childControl != null)
                {
                    childControl.Width = value;
                }
                else
                {
                    base.Width = value;
                }
            }
        }

        private sealed class GenericWebPartControlCollection : ControlCollection
        {
            public GenericWebPartControlCollection(GenericWebPart owner) : base(owner)
            {
                base.SetCollectionReadOnly("GenericWebPart_CannotModify");
            }

            public void AddGenericControl(Control control)
            {
                string errorMsg = base.SetCollectionReadOnly(null);
                try
                {
                    try
                    {
                        this.Clear();
                        this.Add(control);
                    }
                    finally
                    {
                        base.SetCollectionReadOnly(errorMsg);
                    }
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}

