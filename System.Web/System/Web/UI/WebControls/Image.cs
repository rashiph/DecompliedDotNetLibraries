namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [Designer("System.Web.UI.Design.WebControls.PreviewControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("ImageUrl")]
    public class Image : WebControl
    {
        private bool _urlResolved;

        public Image() : base(HtmlTextWriterTag.Img)
        {
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            string str2;
            base.AddAttributesToRender(writer);
            string imageUrl = this.ImageUrl;
            if (!this.UrlResolved)
            {
                imageUrl = base.ResolveClientUrl(imageUrl);
            }
            if ((imageUrl.Length > 0) || !base.EnableLegacyRendering)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, imageUrl);
            }
            imageUrl = this.DescriptionUrl;
            if (imageUrl.Length != 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Longdesc, base.ResolveClientUrl(imageUrl));
            }
            imageUrl = this.AlternateText;
            if ((imageUrl.Length > 0) || this.GenerateEmptyAlternateText)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, imageUrl);
            }
            switch (this.ImageAlign)
            {
                case System.Web.UI.WebControls.ImageAlign.Left:
                    str2 = "left";
                    break;

                case System.Web.UI.WebControls.ImageAlign.Right:
                    str2 = "right";
                    break;

                case System.Web.UI.WebControls.ImageAlign.Baseline:
                    str2 = "baseline";
                    break;

                case System.Web.UI.WebControls.ImageAlign.Top:
                    str2 = "top";
                    break;

                case System.Web.UI.WebControls.ImageAlign.Middle:
                    str2 = "middle";
                    break;

                case System.Web.UI.WebControls.ImageAlign.Bottom:
                    str2 = "bottom";
                    break;

                case System.Web.UI.WebControls.ImageAlign.AbsBottom:
                    str2 = "absbottom";
                    break;

                case System.Web.UI.WebControls.ImageAlign.AbsMiddle:
                    str2 = "absmiddle";
                    break;

                case System.Web.UI.WebControls.ImageAlign.NotSet:
                    goto Label_00FA;

                default:
                    str2 = "texttop";
                    break;
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Align, str2);
        Label_00FA:
            if (this.BorderWidth.IsEmpty && (this.RenderingCompatibility < VersionUtil.Framework40))
            {
                if (base.EnableLegacyRendering)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Border, "0", false);
                    return;
                }
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0px");
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
        }

        [Localizable(true), Bindable(true), WebCategory("Appearance"), DefaultValue(""), WebSysDescription("Image_AlternateText")]
        public virtual string AlternateText
        {
            get
            {
                string str = (string) this.ViewState["AlternateText"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["AlternateText"] = value;
            }
        }

        [UrlProperty, WebCategory("Accessibility"), WebSysDescription("Image_DescriptionUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue("")]
        public virtual string DescriptionUrl
        {
            get
            {
                string str = (string) this.ViewState["DescriptionUrl"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["DescriptionUrl"] = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override FontInfo Font
        {
            get
            {
                return base.Font;
            }
        }

        [WebCategory("Accessibility"), WebSysDescription("Image_GenerateEmptyAlternateText"), DefaultValue(false)]
        public virtual bool GenerateEmptyAlternateText
        {
            get
            {
                object obj2 = this.ViewState["GenerateEmptyAlternateText"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["GenerateEmptyAlternateText"] = value;
            }
        }

        [WebCategory("Layout"), WebSysDescription("Image_ImageAlign"), DefaultValue(0)]
        public virtual System.Web.UI.WebControls.ImageAlign ImageAlign
        {
            get
            {
                object obj2 = this.ViewState["ImageAlign"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.ImageAlign) obj2;
                }
                return System.Web.UI.WebControls.ImageAlign.NotSet;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.ImageAlign.NotSet) || (value > System.Web.UI.WebControls.ImageAlign.TextTop))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["ImageAlign"] = value;
            }
        }

        [Bindable(true), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebSysDescription("Image_ImageUrl"), WebCategory("Appearance"), DefaultValue("")]
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

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return (this.RenderingCompatibility < VersionUtil.Framework40);
            }
        }

        internal bool UrlResolved
        {
            get
            {
                return this._urlResolved;
            }
            set
            {
                this._urlResolved = value;
            }
        }
    }
}

