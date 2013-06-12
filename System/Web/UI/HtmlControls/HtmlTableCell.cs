namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [ConstructorNeedsTag(true)]
    public class HtmlTableCell : HtmlContainerControl
    {
        public HtmlTableCell() : base("td")
        {
        }

        public HtmlTableCell(string tagName) : base(tagName)
        {
        }

        protected override void RenderEndTag(HtmlTextWriter writer)
        {
            base.RenderEndTag(writer);
            writer.WriteLine();
        }

        [WebCategory("Layout"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue("")]
        public string Align
        {
            get
            {
                string str = base.Attributes["align"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["align"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Appearance")]
        public string BgColor
        {
            get
            {
                string str = base.Attributes["bgcolor"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["bgcolor"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [WebCategory("Appearance"), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BorderColor
        {
            get
            {
                string str = base.Attributes["bordercolor"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["bordercolor"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(""), WebCategory("Layout")]
        public int ColSpan
        {
            get
            {
                string s = base.Attributes["colspan"];
                if (s == null)
                {
                    return -1;
                }
                return int.Parse(s, CultureInfo.InvariantCulture);
            }
            set
            {
                base.Attributes["colspan"] = HtmlControl.MapIntegerAttributeToString(value);
            }
        }

        [WebCategory("Layout"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue("")]
        public string Height
        {
            get
            {
                string str = base.Attributes["height"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["height"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [TypeConverter(typeof(MinimizableAttributeTypeConverter)), WebCategory("Behavior"), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool NoWrap
        {
            get
            {
                string str = base.Attributes["nowrap"];
                if (str == null)
                {
                    return false;
                }
                return str.Equals("nowrap");
            }
            set
            {
                if (value)
                {
                    base.Attributes["nowrap"] = "nowrap";
                }
                else
                {
                    base.Attributes["nowrap"] = null;
                }
            }
        }

        [WebCategory("Layout"), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RowSpan
        {
            get
            {
                string s = base.Attributes["rowspan"];
                if (s == null)
                {
                    return -1;
                }
                return int.Parse(s, CultureInfo.InvariantCulture);
            }
            set
            {
                base.Attributes["rowspan"] = HtmlControl.MapIntegerAttributeToString(value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(""), WebCategory("Layout")]
        public string VAlign
        {
            get
            {
                string str = base.Attributes["valign"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["valign"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [WebCategory("Layout"), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Width
        {
            get
            {
                string str = base.Attributes["width"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["width"] = HtmlControl.MapStringAttributeToString(value);
            }
        }
    }
}

