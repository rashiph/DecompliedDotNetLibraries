namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    public class TableStyle : Style
    {
        internal const int PROP_BACKIMAGEURL = 0x10000;
        internal const int PROP_CELLPADDING = 0x20000;
        internal const int PROP_CELLSPACING = 0x40000;
        internal const int PROP_GRIDLINES = 0x80000;
        internal const int PROP_HORZALIGN = 0x100000;

        public TableStyle()
        {
        }

        public TableStyle(StateBag bag) : base(bag)
        {
        }

        public override void AddAttributesToRender(HtmlTextWriter writer, WebControl owner)
        {
            base.AddAttributesToRender(writer, owner);
            int cellSpacing = this.CellSpacing;
            if (cellSpacing >= 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, cellSpacing.ToString(NumberFormatInfo.InvariantInfo));
                if (cellSpacing == 0)
                {
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BorderCollapse, "collapse");
                }
            }
            cellSpacing = this.CellPadding;
            if (cellSpacing >= 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, cellSpacing.ToString(NumberFormatInfo.InvariantInfo));
            }
            System.Web.UI.WebControls.HorizontalAlign horizontalAlign = this.HorizontalAlign;
            if (horizontalAlign != System.Web.UI.WebControls.HorizontalAlign.NotSet)
            {
                string str = "Justify";
                switch (horizontalAlign)
                {
                    case System.Web.UI.WebControls.HorizontalAlign.Left:
                        str = "Left";
                        break;

                    case System.Web.UI.WebControls.HorizontalAlign.Center:
                        str = "Center";
                        break;

                    case System.Web.UI.WebControls.HorizontalAlign.Right:
                        str = "Right";
                        break;

                    case System.Web.UI.WebControls.HorizontalAlign.Justify:
                        str = "Justify";
                        break;
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Align, str);
            }
            if (this.GridLines != System.Web.UI.WebControls.GridLines.None)
            {
                string str2 = string.Empty;
                switch (this.GridLines)
                {
                    case System.Web.UI.WebControls.GridLines.Horizontal:
                        str2 = "rows";
                        break;

                    case System.Web.UI.WebControls.GridLines.Vertical:
                        str2 = "cols";
                        break;

                    case System.Web.UI.WebControls.GridLines.Both:
                        str2 = "all";
                        break;
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Rules, str2);
            }
        }

        public override void CopyFrom(Style s)
        {
            if ((s != null) && !s.IsEmpty)
            {
                base.CopyFrom(s);
                TableStyle style = s as TableStyle;
                if (style != null)
                {
                    if (s.RegisteredCssClass.Length != 0)
                    {
                        if (style.IsSet(0x10000))
                        {
                            base.ViewState.Remove("BackImageUrl");
                            base.ClearBit(0x10000);
                        }
                    }
                    else if (style.IsSet(0x10000))
                    {
                        this.BackImageUrl = style.BackImageUrl;
                    }
                    if (style.IsSet(0x20000))
                    {
                        this.CellPadding = style.CellPadding;
                    }
                    if (style.IsSet(0x40000))
                    {
                        this.CellSpacing = style.CellSpacing;
                    }
                    if (style.IsSet(0x80000))
                    {
                        this.GridLines = style.GridLines;
                    }
                    if (style.IsSet(0x100000))
                    {
                        this.HorizontalAlign = style.HorizontalAlign;
                    }
                }
            }
        }

        protected override void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver)
        {
            base.FillStyleAttributes(attributes, urlResolver);
            string backImageUrl = this.BackImageUrl;
            if (backImageUrl.Length != 0)
            {
                if (urlResolver != null)
                {
                    backImageUrl = urlResolver.ResolveClientUrl(backImageUrl);
                }
                attributes.Add(HtmlTextWriterStyle.BackgroundImage, backImageUrl);
            }
        }

        public override void MergeWith(Style s)
        {
            if ((s != null) && !s.IsEmpty)
            {
                if (this.IsEmpty)
                {
                    this.CopyFrom(s);
                }
                else
                {
                    base.MergeWith(s);
                    TableStyle style = s as TableStyle;
                    if (style != null)
                    {
                        if (((s.RegisteredCssClass.Length == 0) && style.IsSet(0x10000)) && !base.IsSet(0x10000))
                        {
                            this.BackImageUrl = style.BackImageUrl;
                        }
                        if (style.IsSet(0x20000) && !base.IsSet(0x20000))
                        {
                            this.CellPadding = style.CellPadding;
                        }
                        if (style.IsSet(0x40000) && !base.IsSet(0x40000))
                        {
                            this.CellSpacing = style.CellSpacing;
                        }
                        if (style.IsSet(0x80000) && !base.IsSet(0x80000))
                        {
                            this.GridLines = style.GridLines;
                        }
                        if (style.IsSet(0x100000) && !base.IsSet(0x100000))
                        {
                            this.HorizontalAlign = style.HorizontalAlign;
                        }
                    }
                }
            }
        }

        public override void Reset()
        {
            if (base.IsSet(0x10000))
            {
                base.ViewState.Remove("BackImageUrl");
            }
            if (base.IsSet(0x20000))
            {
                base.ViewState.Remove("CellPadding");
            }
            if (base.IsSet(0x40000))
            {
                base.ViewState.Remove("CellSpacing");
            }
            if (base.IsSet(0x80000))
            {
                base.ViewState.Remove("GridLines");
            }
            if (base.IsSet(0x100000))
            {
                base.ViewState.Remove("HorizontalAlign");
            }
            base.Reset();
        }

        [UrlProperty, WebCategory("Appearance"), WebSysDescription("TableStyle_BackImageUrl"), NotifyParentProperty(true), DefaultValue("")]
        public virtual string BackImageUrl
        {
            get
            {
                if (base.IsSet(0x10000))
                {
                    return (string) base.ViewState["BackImageUrl"];
                }
                return string.Empty;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                base.ViewState["BackImageUrl"] = value;
                this.SetBit(0x10000);
            }
        }

        [NotifyParentProperty(true), WebCategory("Appearance"), DefaultValue(-1), WebSysDescription("TableStyle_CellPadding")]
        public virtual int CellPadding
        {
            get
            {
                if (base.IsSet(0x20000))
                {
                    return (int) base.ViewState["CellPadding"];
                }
                return -1;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("TableStyle_InvalidCellPadding"));
                }
                base.ViewState["CellPadding"] = value;
                this.SetBit(0x20000);
            }
        }

        [WebCategory("Appearance"), WebSysDescription("TableStyle_CellSpacing"), NotifyParentProperty(true), DefaultValue(-1)]
        public virtual int CellSpacing
        {
            get
            {
                if (base.IsSet(0x40000))
                {
                    return (int) base.ViewState["CellSpacing"];
                }
                return -1;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("TableStyle_InvalidCellSpacing"));
                }
                base.ViewState["CellSpacing"] = value;
                this.SetBit(0x40000);
            }
        }

        [WebSysDescription("TableStyle_GridLines"), NotifyParentProperty(true), WebCategory("Appearance"), DefaultValue(0)]
        public virtual System.Web.UI.WebControls.GridLines GridLines
        {
            get
            {
                if (base.IsSet(0x80000))
                {
                    return (System.Web.UI.WebControls.GridLines) base.ViewState["GridLines"];
                }
                return System.Web.UI.WebControls.GridLines.None;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.GridLines.None) || (value > System.Web.UI.WebControls.GridLines.Both))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["GridLines"] = value;
                this.SetBit(0x80000);
            }
        }

        [NotifyParentProperty(true), WebCategory("Layout"), DefaultValue(0), WebSysDescription("TableStyle_HorizontalAlign")]
        public virtual System.Web.UI.WebControls.HorizontalAlign HorizontalAlign
        {
            get
            {
                if (base.IsSet(0x100000))
                {
                    return (System.Web.UI.WebControls.HorizontalAlign) base.ViewState["HorizontalAlign"];
                }
                return System.Web.UI.WebControls.HorizontalAlign.NotSet;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.HorizontalAlign.NotSet) || (value > System.Web.UI.WebControls.HorizontalAlign.Justify))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["HorizontalAlign"] = value;
                this.SetBit(0x100000);
            }
        }
    }
}

