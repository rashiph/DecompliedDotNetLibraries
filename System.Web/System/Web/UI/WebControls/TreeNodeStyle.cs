namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    public sealed class TreeNodeStyle : Style
    {
        private System.Web.UI.WebControls.HyperLinkStyle _hyperLinkStyle;
        private const int PROP_CHILDNODESPADDING = 0x80000;
        private const int PROP_HPADDING = 0x20000;
        private const int PROP_IMAGEURL = 0x100000;
        private const int PROP_NODESPACING = 0x40000;
        private const int PROP_VPADDING = 0x10000;

        public TreeNodeStyle()
        {
        }

        public TreeNodeStyle(StateBag bag) : base(bag)
        {
        }

        public override void CopyFrom(Style s)
        {
            if (s != null)
            {
                base.CopyFrom(s);
                TreeNodeStyle style = s as TreeNodeStyle;
                if ((style != null) && !style.IsEmpty)
                {
                    if (s.RegisteredCssClass.Length != 0)
                    {
                        if (style.IsSet(0x10000))
                        {
                            base.ViewState.Remove("VerticalPadding");
                            base.ClearBit(0x10000);
                        }
                        if (style.IsSet(0x20000))
                        {
                            base.ViewState.Remove("HorizontalPadding");
                            base.ClearBit(0x20000);
                        }
                    }
                    else
                    {
                        if (style.IsSet(0x10000))
                        {
                            this.VerticalPadding = style.VerticalPadding;
                        }
                        if (style.IsSet(0x20000))
                        {
                            this.HorizontalPadding = style.HorizontalPadding;
                        }
                    }
                    if (style.IsSet(0x40000))
                    {
                        this.NodeSpacing = style.NodeSpacing;
                    }
                    if (style.IsSet(0x80000))
                    {
                        this.ChildNodesPadding = style.ChildNodesPadding;
                    }
                    if (style.IsSet(0x100000))
                    {
                        this.ImageUrl = style.ImageUrl;
                    }
                }
            }
        }

        protected override void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver)
        {
            Color color;
            Unit unit2;
            StateBag viewState = base.ViewState;
            if (base.IsSet(8))
            {
                color = (Color) viewState["BackColor"];
                if (!color.IsEmpty)
                {
                    attributes.Add(HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(color));
                }
            }
            if (base.IsSet(0x10))
            {
                color = (Color) viewState["BorderColor"];
                if (!color.IsEmpty)
                {
                    attributes.Add(HtmlTextWriterStyle.BorderColor, ColorTranslator.ToHtml(color));
                }
            }
            BorderStyle borderStyle = base.BorderStyle;
            Unit borderWidth = base.BorderWidth;
            if (!borderWidth.IsEmpty)
            {
                attributes.Add(HtmlTextWriterStyle.BorderWidth, borderWidth.ToString(CultureInfo.InvariantCulture));
                if (borderStyle == BorderStyle.NotSet)
                {
                    if (borderWidth.Value != 0.0)
                    {
                        attributes.Add(HtmlTextWriterStyle.BorderStyle, "solid");
                    }
                }
                else
                {
                    attributes.Add(HtmlTextWriterStyle.BorderStyle, Style.borderStyles[(int) borderStyle]);
                }
            }
            else if (borderStyle != BorderStyle.NotSet)
            {
                attributes.Add(HtmlTextWriterStyle.BorderStyle, Style.borderStyles[(int) borderStyle]);
            }
            if (base.IsSet(0x80))
            {
                unit2 = (Unit) viewState["Height"];
                if (!unit2.IsEmpty)
                {
                    attributes.Add(HtmlTextWriterStyle.Height, unit2.ToString(CultureInfo.InvariantCulture));
                }
            }
            if (base.IsSet(0x100))
            {
                unit2 = (Unit) viewState["Width"];
                if (!unit2.IsEmpty)
                {
                    attributes.Add(HtmlTextWriterStyle.Width, unit2.ToString(CultureInfo.InvariantCulture));
                }
            }
            if (!this.HorizontalPadding.IsEmpty || !this.VerticalPadding.IsEmpty)
            {
                attributes.Add(HtmlTextWriterStyle.Padding, string.Format(CultureInfo.InvariantCulture, "{0} {1} {0} {1}", new object[] { this.VerticalPadding.IsEmpty ? Unit.Pixel(0) : this.VerticalPadding, this.HorizontalPadding.IsEmpty ? Unit.Pixel(0) : this.HorizontalPadding }));
            }
        }

        public override void MergeWith(Style s)
        {
            if (s != null)
            {
                if (this.IsEmpty)
                {
                    this.CopyFrom(s);
                }
                else
                {
                    base.MergeWith(s);
                    TreeNodeStyle style = s as TreeNodeStyle;
                    if ((style != null) && !style.IsEmpty)
                    {
                        if (s.RegisteredCssClass.Length == 0)
                        {
                            if (style.IsSet(0x10000) && !base.IsSet(0x10000))
                            {
                                this.VerticalPadding = style.VerticalPadding;
                            }
                            if (style.IsSet(0x20000) && !base.IsSet(0x20000))
                            {
                                this.HorizontalPadding = style.HorizontalPadding;
                            }
                        }
                        if (style.IsSet(0x40000) && !base.IsSet(0x40000))
                        {
                            this.NodeSpacing = style.NodeSpacing;
                        }
                        if (style.IsSet(0x80000) && !base.IsSet(0x80000))
                        {
                            this.ChildNodesPadding = style.ChildNodesPadding;
                        }
                        if (style.IsSet(0x100000) && !base.IsSet(0x100000))
                        {
                            this.ImageUrl = style.ImageUrl;
                        }
                    }
                }
            }
        }

        public override void Reset()
        {
            if (base.IsSet(0x10000))
            {
                base.ViewState.Remove("VerticalPadding");
            }
            if (base.IsSet(0x20000))
            {
                base.ViewState.Remove("HorizontalPadding");
            }
            if (base.IsSet(0x40000))
            {
                base.ViewState.Remove("NodeSpacing");
            }
            if (base.IsSet(0x80000))
            {
                base.ViewState.Remove("ChildNodesPadding");
            }
            this.ResetCachedStyles();
            base.Reset();
        }

        internal void ResetCachedStyles()
        {
            this._hyperLinkStyle = null;
        }

        [DefaultValue(typeof(Unit), ""), WebSysDescription("TreeNodeStyle_ChildNodesPadding"), WebCategory("Layout"), NotifyParentProperty(true)]
        public Unit ChildNodesPadding
        {
            get
            {
                if (base.IsSet(0x80000))
                {
                    return (Unit) base.ViewState["ChildNodesPadding"];
                }
                return Unit.Empty;
            }
            set
            {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0.0))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["ChildNodesPadding"] = value;
                this.SetBit(0x80000);
            }
        }

        [DefaultValue(typeof(Unit), ""), NotifyParentProperty(true), WebSysDescription("TreeNodeStyle_HorizontalPadding"), WebCategory("Layout")]
        public Unit HorizontalPadding
        {
            get
            {
                if (base.IsSet(0x20000))
                {
                    return (Unit) base.ViewState["HorizontalPadding"];
                }
                return Unit.Empty;
            }
            set
            {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0.0))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["HorizontalPadding"] = value;
                this.SetBit(0x20000);
            }
        }

        internal System.Web.UI.WebControls.HyperLinkStyle HyperLinkStyle
        {
            get
            {
                if (this._hyperLinkStyle == null)
                {
                    this._hyperLinkStyle = new System.Web.UI.WebControls.HyperLinkStyle(this);
                }
                return this._hyperLinkStyle;
            }
        }

        [WebSysDescription("TreeNodeStyle_ImageUrl"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), NotifyParentProperty(true), UrlProperty, WebCategory("Appearance")]
        public string ImageUrl
        {
            get
            {
                if (base.IsSet(0x100000))
                {
                    return (string) base.ViewState["ImageUrl"];
                }
                return string.Empty;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                base.ViewState["ImageUrl"] = value;
                this.SetBit(0x100000);
            }
        }

        [WebCategory("Layout"), DefaultValue(typeof(Unit), ""), WebSysDescription("TreeNodeStyle_NodeSpacing"), NotifyParentProperty(true)]
        public Unit NodeSpacing
        {
            get
            {
                if (base.IsSet(0x40000))
                {
                    return (Unit) base.ViewState["NodeSpacing"];
                }
                return Unit.Empty;
            }
            set
            {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0.0))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["NodeSpacing"] = value;
                this.SetBit(0x40000);
            }
        }

        [NotifyParentProperty(true), DefaultValue(typeof(Unit), ""), WebSysDescription("TreeNodeStyle_VerticalPadding"), WebCategory("Layout")]
        public Unit VerticalPadding
        {
            get
            {
                if (base.IsSet(0x10000))
                {
                    return (Unit) base.ViewState["VerticalPadding"];
                }
                return Unit.Empty;
            }
            set
            {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0.0))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["VerticalPadding"] = value;
                this.SetBit(0x10000);
            }
        }
    }
}

