namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [ToolboxData("<{0}:HyperLink runat=\"server\">HyperLink</{0}:HyperLink>"), ParseChildren(false), ControlBuilder(typeof(HyperLinkControlBuilder)), DataBindingHandler("System.Web.UI.Design.HyperLinkDataBindingHandler, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("Text"), Designer("System.Web.UI.Design.WebControls.HyperLinkDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class HyperLink : WebControl
    {
        private bool _textSetByAddParsedSubObject;

        public HyperLink() : base(HtmlTextWriterTag.A)
        {
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            if ((this.Enabled && !base.IsEnabled) && this.SupportsDisabledAttribute)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }
            base.AddAttributesToRender(writer);
            string navigateUrl = this.NavigateUrl;
            if ((navigateUrl.Length > 0) && base.IsEnabled)
            {
                string str2 = base.ResolveClientUrl(navigateUrl);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, str2);
            }
            navigateUrl = this.Target;
            if (navigateUrl.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Target, navigateUrl);
            }
        }

        protected override void AddParsedSubObject(object obj)
        {
            if (this.HasControls())
            {
                base.AddParsedSubObject(obj);
            }
            else if (obj is LiteralControl)
            {
                if (this._textSetByAddParsedSubObject)
                {
                    this.Text = this.Text + ((LiteralControl) obj).Text;
                }
                else
                {
                    this.Text = ((LiteralControl) obj).Text;
                }
                this._textSetByAddParsedSubObject = true;
            }
            else
            {
                string text = this.Text;
                if (text.Length != 0)
                {
                    this.Text = null;
                    base.AddParsedSubObject(new LiteralControl(text));
                }
                base.AddParsedSubObject(obj);
            }
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                base.LoadViewState(savedState);
                if ((((string) this.ViewState["Text"]) != null) && this.HasControls())
                {
                    this.Controls.Clear();
                }
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            string imageUrl = this.ImageUrl;
            if (imageUrl.Length > 0)
            {
                Image image = new Image {
                    ImageUrl = base.ResolveClientUrl(imageUrl),
                    UrlResolved = true,
                    GenerateEmptyAlternateText = true
                };
                imageUrl = this.ToolTip;
                if (imageUrl.Length != 0)
                {
                    image.ToolTip = imageUrl;
                }
                imageUrl = this.Text;
                if (imageUrl.Length != 0)
                {
                    image.AlternateText = imageUrl;
                }
                image.RenderControl(writer);
            }
            else if (base.HasRenderingData())
            {
                base.RenderContents(writer);
            }
            else
            {
                writer.Write(this.Text);
            }
        }

        [WebCategory("Appearance"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebSysDescription("HyperLink_ImageUrl"), Bindable(true)]
        public virtual string ImageUrl
        {
            get
            {
                string str = (string) this.ViewState["ImageUrl"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["ImageUrl"] = value;
            }
        }

        [WebSysDescription("HyperLink_NavigateUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, Bindable(true), WebCategory("Navigation"), DefaultValue("")]
        public string NavigateUrl
        {
            get
            {
                string str = (string) this.ViewState["NavigateUrl"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["NavigateUrl"] = value;
            }
        }

        internal override bool RequiresLegacyRendering
        {
            get
            {
                return true;
            }
        }

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return (this.RenderingCompatibility < VersionUtil.Framework40);
            }
        }

        [DefaultValue(""), TypeConverter(typeof(TargetConverter)), WebCategory("Navigation"), WebSysDescription("HyperLink_Target")]
        public string Target
        {
            get
            {
                string str = (string) this.ViewState["Target"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["Target"] = value;
            }
        }

        [DefaultValue(""), Bindable(true), WebCategory("Appearance"), Localizable(true), WebSysDescription("HyperLink_Text"), PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public virtual string Text
        {
            get
            {
                object obj2 = this.ViewState["Text"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (this.HasControls())
                {
                    this.Controls.Clear();
                }
                this.ViewState["Text"] = value;
            }
        }
    }
}

