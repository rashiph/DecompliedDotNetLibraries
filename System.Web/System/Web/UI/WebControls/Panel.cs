namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [ParseChildren(false), PersistChildren(true), Designer("System.Web.UI.Design.WebControls.PanelContainerDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class Panel : WebControl
    {
        private string _defaultButton;
        private bool _renderedFieldSet;

        public Panel() : base(HtmlTextWriterTag.Div)
        {
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            string backImageUrl = this.BackImageUrl;
            if (backImageUrl.Trim().Length > 0)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundImage, "url(" + base.ResolveClientUrl(backImageUrl) + ")");
            }
            this.AddScrollingAttribute(this.ScrollBars, writer);
            System.Web.UI.WebControls.HorizontalAlign horizontalAlign = this.HorizontalAlign;
            if (horizontalAlign != System.Web.UI.WebControls.HorizontalAlign.NotSet)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(System.Web.UI.WebControls.HorizontalAlign));
                writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, converter.ConvertToInvariantString(horizontalAlign).ToLowerInvariant());
            }
            if (!this.Wrap)
            {
                if (base.EnableLegacyRendering)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Nowrap, "nowrap", false);
                }
                else
                {
                    writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                }
            }
            if (this.Direction == ContentDirection.LeftToRight)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Dir, "ltr");
            }
            else if (this.Direction == ContentDirection.RightToLeft)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Dir, "rtl");
            }
            if (((!base.DesignMode && (this.Page != null)) && ((this.Page.RequestInternal != null) && (this.Page.Request.Browser.EcmaScriptVersion.Major > 0))) && ((this.Page.Request.Browser.W3CDomVersion.Major > 0) && (this.DefaultButton.Length > 0)))
            {
                Control button = this.FindControl(this.DefaultButton);
                if (!(button is IButtonControl))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("HtmlForm_OnlyIButtonControlCanBeDefaultButton", new object[] { this.ID }));
                }
                this.Page.ClientScript.RegisterDefaultButtonScript(button, writer, true);
            }
        }

        private void AddScrollingAttribute(System.Web.UI.WebControls.ScrollBars scrollBars, HtmlTextWriter writer)
        {
            switch (scrollBars)
            {
                case System.Web.UI.WebControls.ScrollBars.Horizontal:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.OverflowX, "scroll");
                    return;

                case System.Web.UI.WebControls.ScrollBars.Vertical:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.OverflowY, "scroll");
                    return;

                case System.Web.UI.WebControls.ScrollBars.Both:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Overflow, "scroll");
                    return;

                case System.Web.UI.WebControls.ScrollBars.Auto:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Overflow, "auto");
                    return;
            }
        }

        protected override Style CreateControlStyle()
        {
            return new PanelStyle(this.ViewState);
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            this.AddAttributesToRender(writer);
            HtmlTextWriterTag tagKey = this.TagKey;
            if (tagKey != HtmlTextWriterTag.Unknown)
            {
                writer.RenderBeginTag(tagKey);
            }
            else
            {
                writer.RenderBeginTag(this.TagName);
            }
            string groupingText = this.GroupingText;
            if ((groupingText.Length != 0) && !(writer is Html32TextWriter))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);
                this._renderedFieldSet = true;
                writer.RenderBeginTag(HtmlTextWriterTag.Legend);
                writer.Write(groupingText);
                writer.RenderEndTag();
            }
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            if (this._renderedFieldSet)
            {
                writer.RenderEndTag();
            }
            base.RenderEndTag(writer);
        }

        [WebCategory("Appearance"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebSysDescription("Panel_BackImageUrl"), DefaultValue("")]
        public virtual string BackImageUrl
        {
            get
            {
                if (base.ControlStyleCreated)
                {
                    PanelStyle controlStyle = base.ControlStyle as PanelStyle;
                    if (controlStyle != null)
                    {
                        return controlStyle.BackImageUrl;
                    }
                    string str = (string) this.ViewState["BackImageUrl"];
                    if (str != null)
                    {
                        return str;
                    }
                }
                return string.Empty;
            }
            set
            {
                PanelStyle controlStyle = base.ControlStyle as PanelStyle;
                if (controlStyle != null)
                {
                    controlStyle.BackImageUrl = value;
                }
                else
                {
                    this.ViewState["BackImageUrl"] = value;
                }
            }
        }

        [Themeable(false), WebSysDescription("Panel_DefaultButton"), DefaultValue(""), WebCategory("Behavior")]
        public virtual string DefaultButton
        {
            get
            {
                if (this._defaultButton == null)
                {
                    return string.Empty;
                }
                return this._defaultButton;
            }
            set
            {
                this._defaultButton = value;
            }
        }

        [DefaultValue(0), WebSysDescription("Panel_Direction"), WebCategory("Layout")]
        public virtual ContentDirection Direction
        {
            get
            {
                if (base.ControlStyleCreated)
                {
                    PanelStyle controlStyle = base.ControlStyle as PanelStyle;
                    if (controlStyle != null)
                    {
                        return controlStyle.Direction;
                    }
                    object obj2 = this.ViewState["Direction"];
                    if (obj2 != null)
                    {
                        return (ContentDirection) obj2;
                    }
                }
                return ContentDirection.NotSet;
            }
            set
            {
                PanelStyle controlStyle = base.ControlStyle as PanelStyle;
                if (controlStyle != null)
                {
                    controlStyle.Direction = value;
                }
                else
                {
                    this.ViewState["Direction"] = value;
                }
            }
        }

        [WebSysDescription("Panel_GroupingText"), Localizable(true), DefaultValue(""), WebCategory("Appearance")]
        public virtual string GroupingText
        {
            get
            {
                string str = (string) this.ViewState["GroupingText"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["GroupingText"] = value;
            }
        }

        [DefaultValue(0), WebCategory("Layout"), WebSysDescription("Panel_HorizontalAlign")]
        public virtual System.Web.UI.WebControls.HorizontalAlign HorizontalAlign
        {
            get
            {
                if (base.ControlStyleCreated)
                {
                    PanelStyle controlStyle = base.ControlStyle as PanelStyle;
                    if (controlStyle != null)
                    {
                        return controlStyle.HorizontalAlign;
                    }
                    object obj2 = this.ViewState["HorizontalAlign"];
                    if (obj2 != null)
                    {
                        return (System.Web.UI.WebControls.HorizontalAlign) obj2;
                    }
                }
                return System.Web.UI.WebControls.HorizontalAlign.NotSet;
            }
            set
            {
                PanelStyle controlStyle = base.ControlStyle as PanelStyle;
                if (controlStyle != null)
                {
                    controlStyle.HorizontalAlign = value;
                }
                else
                {
                    if ((value < System.Web.UI.WebControls.HorizontalAlign.NotSet) || (value > System.Web.UI.WebControls.HorizontalAlign.Justify))
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    this.ViewState["HorizontalAlign"] = value;
                }
            }
        }

        [WebCategory("Layout"), WebSysDescription("Panel_ScrollBars"), DefaultValue(0)]
        public virtual System.Web.UI.WebControls.ScrollBars ScrollBars
        {
            get
            {
                if (base.ControlStyleCreated)
                {
                    PanelStyle controlStyle = base.ControlStyle as PanelStyle;
                    if (controlStyle != null)
                    {
                        return controlStyle.ScrollBars;
                    }
                    object obj2 = this.ViewState["ScrollBars"];
                    if (obj2 != null)
                    {
                        return (System.Web.UI.WebControls.ScrollBars) obj2;
                    }
                }
                return System.Web.UI.WebControls.ScrollBars.None;
            }
            set
            {
                PanelStyle controlStyle = base.ControlStyle as PanelStyle;
                if (controlStyle != null)
                {
                    controlStyle.ScrollBars = value;
                }
                else
                {
                    this.ViewState["ScrollBars"] = value;
                }
            }
        }

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return (this.RenderingCompatibility < VersionUtil.Framework40);
            }
        }

        [WebSysDescription("Panel_Wrap"), WebCategory("Layout"), DefaultValue(true)]
        public virtual bool Wrap
        {
            get
            {
                if (base.ControlStyleCreated)
                {
                    PanelStyle controlStyle = base.ControlStyle as PanelStyle;
                    if (controlStyle != null)
                    {
                        return controlStyle.Wrap;
                    }
                    object obj2 = this.ViewState["Wrap"];
                    if (obj2 != null)
                    {
                        return (bool) obj2;
                    }
                }
                return true;
            }
            set
            {
                PanelStyle controlStyle = base.ControlStyle as PanelStyle;
                if (controlStyle != null)
                {
                    controlStyle.Wrap = value;
                }
                else
                {
                    this.ViewState["Wrap"] = value;
                }
            }
        }
    }
}

