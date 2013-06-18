namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;

    public class TableItemStyle : Style
    {
        internal const int PROP_HORZALIGN = 0x10000;
        internal const int PROP_VERTALIGN = 0x20000;
        internal const int PROP_WRAP = 0x40000;

        public TableItemStyle()
        {
        }

        public TableItemStyle(StateBag bag) : base(bag)
        {
        }

        public override void AddAttributesToRender(HtmlTextWriter writer, WebControl owner)
        {
            base.AddAttributesToRender(writer, owner);
            if (!this.Wrap)
            {
                if (this.IsControlEnableLegacyRendering(owner))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Nowrap, "nowrap");
                }
                else
                {
                    writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                }
            }
            System.Web.UI.WebControls.HorizontalAlign horizontalAlign = this.HorizontalAlign;
            if (horizontalAlign != System.Web.UI.WebControls.HorizontalAlign.NotSet)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(System.Web.UI.WebControls.HorizontalAlign));
                writer.AddAttribute(HtmlTextWriterAttribute.Align, converter.ConvertToString(horizontalAlign).ToLower(CultureInfo.InvariantCulture));
            }
            System.Web.UI.WebControls.VerticalAlign verticalAlign = this.VerticalAlign;
            if (verticalAlign != System.Web.UI.WebControls.VerticalAlign.NotSet)
            {
                TypeConverter converter2 = TypeDescriptor.GetConverter(typeof(System.Web.UI.WebControls.VerticalAlign));
                writer.AddAttribute(HtmlTextWriterAttribute.Valign, converter2.ConvertToString(verticalAlign).ToLower(CultureInfo.InvariantCulture));
            }
        }

        public override void CopyFrom(Style s)
        {
            if ((s != null) && !s.IsEmpty)
            {
                base.CopyFrom(s);
                if (s is TableItemStyle)
                {
                    TableItemStyle style = (TableItemStyle) s;
                    if (s.RegisteredCssClass.Length != 0)
                    {
                        if (style.IsSet(0x40000))
                        {
                            base.ViewState.Remove("Wrap");
                            base.ClearBit(0x40000);
                        }
                    }
                    else if (style.IsSet(0x40000))
                    {
                        this.Wrap = style.Wrap;
                    }
                    if (style.IsSet(0x10000))
                    {
                        this.HorizontalAlign = style.HorizontalAlign;
                    }
                    if (style.IsSet(0x20000))
                    {
                        this.VerticalAlign = style.VerticalAlign;
                    }
                }
            }
        }

        private bool IsControlEnableLegacyRendering(Control control)
        {
            if (control != null)
            {
                return control.EnableLegacyRendering;
            }
            return this.EnableLegacyRendering;
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
                    if (s is TableItemStyle)
                    {
                        TableItemStyle style = (TableItemStyle) s;
                        if (((s.RegisteredCssClass.Length == 0) && style.IsSet(0x40000)) && !base.IsSet(0x40000))
                        {
                            this.Wrap = style.Wrap;
                        }
                        if (style.IsSet(0x10000) && !base.IsSet(0x10000))
                        {
                            this.HorizontalAlign = style.HorizontalAlign;
                        }
                        if (style.IsSet(0x20000) && !base.IsSet(0x20000))
                        {
                            this.VerticalAlign = style.VerticalAlign;
                        }
                    }
                }
            }
        }

        public override void Reset()
        {
            if (base.IsSet(0x10000))
            {
                base.ViewState.Remove("HorizontalAlign");
            }
            if (base.IsSet(0x20000))
            {
                base.ViewState.Remove("VerticalAlign");
            }
            if (base.IsSet(0x40000))
            {
                base.ViewState.Remove("Wrap");
            }
            base.Reset();
        }

        private void ResetWrap()
        {
            base.ViewState.Remove("Wrap");
            base.ClearBit(0x40000);
        }

        private bool ShouldSerializeWrap()
        {
            return base.IsSet(0x40000);
        }

        private bool EnableLegacyRendering
        {
            get
            {
                return (RuntimeConfig.GetAppConfig().XhtmlConformance.Mode == XhtmlConformanceMode.Legacy);
            }
        }

        [NotifyParentProperty(true), WebCategory("Layout"), DefaultValue(0), WebSysDescription("TableItem_HorizontalAlign")]
        public virtual System.Web.UI.WebControls.HorizontalAlign HorizontalAlign
        {
            get
            {
                if (base.IsSet(0x10000))
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
                this.SetBit(0x10000);
            }
        }

        [WebCategory("Layout"), NotifyParentProperty(true), DefaultValue(0), WebSysDescription("TableItem_VerticalAlign")]
        public virtual System.Web.UI.WebControls.VerticalAlign VerticalAlign
        {
            get
            {
                if (base.IsSet(0x20000))
                {
                    return (System.Web.UI.WebControls.VerticalAlign) base.ViewState["VerticalAlign"];
                }
                return System.Web.UI.WebControls.VerticalAlign.NotSet;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.VerticalAlign.NotSet) || (value > System.Web.UI.WebControls.VerticalAlign.Bottom))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["VerticalAlign"] = value;
                this.SetBit(0x20000);
            }
        }

        [DefaultValue(true), WebCategory("Layout"), NotifyParentProperty(true), WebSysDescription("TableItemStyle_Wrap")]
        public virtual bool Wrap
        {
            get
            {
                if (base.IsSet(0x40000))
                {
                    return (bool) base.ViewState["Wrap"];
                }
                return true;
            }
            set
            {
                base.ViewState["Wrap"] = value;
                this.SetBit(0x40000);
            }
        }
    }
}

