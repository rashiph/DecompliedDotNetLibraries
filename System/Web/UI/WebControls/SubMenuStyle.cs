namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    public class SubMenuStyle : Style, ICustomTypeDescriptor
    {
        private const int PROP_HPADDING = 0x20000;
        private const int PROP_VPADDING = 0x10000;

        public SubMenuStyle()
        {
        }

        public SubMenuStyle(StateBag bag) : base(bag)
        {
        }

        public override void CopyFrom(Style s)
        {
            if (s != null)
            {
                base.CopyFrom(s);
                SubMenuStyle style = s as SubMenuStyle;
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
                    SubMenuStyle style = s as SubMenuStyle;
                    if (((style != null) && !style.IsEmpty) && (s.RegisteredCssClass.Length == 0))
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
            base.Reset();
        }

        System.ComponentModel.AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor) this).GetProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(base.GetType(), attributes);
            PropertyDescriptor[] descriptorArray = new PropertyDescriptor[properties.Count];
            PropertyDescriptor descriptor = properties["Font"];
            PropertyDescriptor descriptor2 = properties["ForeColor"];
            Attribute[] attributeArray = new Attribute[] { new BrowsableAttribute(false), new EditorBrowsableAttribute(EditorBrowsableState.Never), new ThemeableAttribute(false) };
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = properties[i];
                if ((oldPropertyDescriptor == descriptor) || (oldPropertyDescriptor == descriptor2))
                {
                    descriptorArray[i] = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, attributeArray);
                }
                else
                {
                    descriptorArray[i] = oldPropertyDescriptor;
                }
            }
            return new PropertyDescriptorCollection(descriptorArray, true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        [NotifyParentProperty(true), WebCategory("Layout"), DefaultValue(typeof(Unit), ""), WebSysDescription("SubMenuStyle_HorizontalPadding")]
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

        [DefaultValue(typeof(Unit), ""), WebCategory("Layout"), NotifyParentProperty(true), WebSysDescription("SubMenuStyle_VerticalPadding")]
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

