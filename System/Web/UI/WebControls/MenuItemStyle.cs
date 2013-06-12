namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    public sealed class MenuItemStyle : Style
    {
        private System.Web.UI.WebControls.HyperLinkStyle _hyperLinkStyle;
        private const int PROP_HPADDING = 0x20000;
        private const int PROP_ITEMSPACING = 0x40000;
        private const int PROP_VPADDING = 0x10000;

        public MenuItemStyle()
        {
        }

        public MenuItemStyle(StateBag bag) : base(bag)
        {
        }

        public override void CopyFrom(Style s)
        {
            if (s != null)
            {
                base.CopyFrom(s);
                MenuItemStyle style = s as MenuItemStyle;
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
                        this.ItemSpacing = style.ItemSpacing;
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
                Unit unit3 = this.VerticalPadding.IsEmpty ? Unit.Pixel(0) : this.VerticalPadding;
                Unit unit4 = this.HorizontalPadding.IsEmpty ? Unit.Pixel(0) : this.HorizontalPadding;
                attributes.Add(HtmlTextWriterStyle.Padding, string.Format(CultureInfo.InvariantCulture, "{0} {1} {0} {1}", new object[] { unit3.ToString(CultureInfo.InvariantCulture), unit4.ToString(CultureInfo.InvariantCulture) }));
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
                    MenuItemStyle style = s as MenuItemStyle;
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
                            this.ItemSpacing = style.ItemSpacing;
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
                base.ViewState.Remove("ItemSpacing");
            }
            this.ResetCachedStyles();
            base.Reset();
        }

        internal void ResetCachedStyles()
        {
            this._hyperLinkStyle = null;
        }

        [DefaultValue(typeof(Unit), ""), WebCategory("Layout"), NotifyParentProperty(true), WebSysDescription("MenuItemStyle_HorizontalPadding")]
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

        [DefaultValue(typeof(Unit), ""), WebSysDescription("MenuItemStyle_ItemSpacing"), WebCategory("Layout"), NotifyParentProperty(true)]
        public Unit ItemSpacing
        {
            get
            {
                if (base.IsSet(0x40000))
                {
                    return (Unit) base.ViewState["ItemSpacing"];
                }
                return Unit.Empty;
            }
            set
            {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0.0))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["ItemSpacing"] = value;
                this.SetBit(0x40000);
            }
        }

        [NotifyParentProperty(true), DefaultValue(typeof(Unit), ""), WebCategory("Layout"), WebSysDescription("MenuItemStyle_VerticalPadding")]
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

