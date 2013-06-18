namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public sealed class WebPartMenuStyle : TableStyle, ICustomTypeDescriptor
    {
        private const int PROP_SHADOWCOLOR = 0x200000;

        public WebPartMenuStyle() : this(null)
        {
        }

        public WebPartMenuStyle(StateBag bag) : base(bag)
        {
            this.CellPadding = 1;
            this.CellSpacing = 0;
        }

        public override void CopyFrom(Style s)
        {
            if ((s != null) && !s.IsEmpty)
            {
                base.CopyFrom(s);
                if (s is WebPartMenuStyle)
                {
                    WebPartMenuStyle style = (WebPartMenuStyle) s;
                    if (s.RegisteredCssClass.Length != 0)
                    {
                        if (style.IsSet(0x200000))
                        {
                            base.ViewState.Remove("ShadowColor");
                            base.ClearBit(0x200000);
                        }
                    }
                    else if (style.IsSet(0x200000))
                    {
                        this.ShadowColor = style.ShadowColor;
                    }
                }
            }
        }

        protected override void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver)
        {
            base.FillStyleAttributes(attributes, urlResolver);
            Color shadowColor = this.ShadowColor;
            if (!shadowColor.IsEmpty)
            {
                string str = ColorTranslator.ToHtml(shadowColor);
                string str2 = "progid:DXImageTransform.Microsoft.Shadow(color='" + str + "', Direction=135, Strength=3)";
                attributes.Add(HtmlTextWriterStyle.Filter, str2);
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
                    if (s is WebPartMenuStyle)
                    {
                        WebPartMenuStyle style = (WebPartMenuStyle) s;
                        if (((s.RegisteredCssClass.Length == 0) && style.IsSet(0x200000)) && !base.IsSet(0x200000))
                        {
                            this.ShadowColor = style.ShadowColor;
                        }
                    }
                }
            }
        }

        public override void Reset()
        {
            if (base.IsSet(0x200000))
            {
                base.ViewState.Remove("ShadowColor");
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
            PropertyDescriptor oldPropertyDescriptor = properties["CellPadding"];
            PropertyDescriptor descriptor2 = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, new Attribute[] { new DefaultValueAttribute(1) });
            PropertyDescriptor descriptor3 = properties["CellSpacing"];
            PropertyDescriptor descriptor4 = TypeDescriptor.CreateProperty(base.GetType(), descriptor3, new Attribute[] { new DefaultValueAttribute(0) });
            PropertyDescriptor descriptor5 = properties["Font"];
            PropertyDescriptor descriptor6 = TypeDescriptor.CreateProperty(base.GetType(), descriptor5, new Attribute[] { new BrowsableAttribute(false), new ThemeableAttribute(false), new EditorBrowsableAttribute(EditorBrowsableState.Never) });
            PropertyDescriptor descriptor7 = properties["ForeColor"];
            PropertyDescriptor descriptor8 = TypeDescriptor.CreateProperty(base.GetType(), descriptor7, new Attribute[] { new BrowsableAttribute(false), new ThemeableAttribute(false), new EditorBrowsableAttribute(EditorBrowsableState.Never) });
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDescriptor descriptor9 = properties[i];
                if (descriptor9 == oldPropertyDescriptor)
                {
                    descriptorArray[i] = descriptor2;
                }
                else if (descriptor9 == descriptor3)
                {
                    descriptorArray[i] = descriptor4;
                }
                else if (descriptor9 == descriptor5)
                {
                    descriptorArray[i] = descriptor6;
                }
                else if (descriptor9 == descriptor7)
                {
                    descriptorArray[i] = descriptor8;
                }
                else
                {
                    descriptorArray[i] = descriptor9;
                }
            }
            return new PropertyDescriptorCollection(descriptorArray, true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override System.Web.UI.WebControls.HorizontalAlign HorizontalAlign
        {
            get
            {
                return base.HorizontalAlign;
            }
            set
            {
            }
        }

        [TypeConverter(typeof(WebColorConverter)), WebSysDescription("WebPartMenuStyle_ShadowColor"), DefaultValue(typeof(Color), ""), WebCategory("Appearance")]
        public Color ShadowColor
        {
            get
            {
                if (base.IsSet(0x200000))
                {
                    return (Color) base.ViewState["ShadowColor"];
                }
                return Color.Empty;
            }
            set
            {
                base.ViewState["ShadowColor"] = value;
                this.SetBit(0x200000);
            }
        }
    }
}

