namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [ControlBuilder(typeof(HtmlEmptyTagControlBuilder))]
    public class HtmlImage : HtmlControl
    {
        public HtmlImage() : base("img")
        {
        }

        protected override void RenderAttributes(HtmlTextWriter writer)
        {
            base.PreProcessRelativeReferenceAttribute(writer, "src");
            base.RenderAttributes(writer);
            writer.Write(" /");
        }

        [WebCategory("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue("")]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Appearance"), Localizable(true), DefaultValue("")]
        public string Alt
        {
            get
            {
                string str = base.Attributes["alt"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["alt"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Appearance"), DefaultValue(0)]
        public int Border
        {
            get
            {
                string s = base.Attributes["border"];
                if (s == null)
                {
                    return -1;
                }
                return int.Parse(s, CultureInfo.InvariantCulture);
            }
            set
            {
                base.Attributes["border"] = HtmlControl.MapIntegerAttributeToString(value);
            }
        }

        [WebCategory("Layout"), DefaultValue(100), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Height
        {
            get
            {
                string s = base.Attributes["height"];
                if (s == null)
                {
                    return -1;
                }
                return int.Parse(s, CultureInfo.InvariantCulture);
            }
            set
            {
                base.Attributes["height"] = HtmlControl.MapIntegerAttributeToString(value);
            }
        }

        [UrlProperty, DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Behavior"), DefaultValue("")]
        public string Src
        {
            get
            {
                string str = base.Attributes["src"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["src"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(100), WebCategory("Layout")]
        public int Width
        {
            get
            {
                string s = base.Attributes["width"];
                if (s == null)
                {
                    return -1;
                }
                return int.Parse(s, CultureInfo.InvariantCulture);
            }
            set
            {
                base.Attributes["width"] = HtmlControl.MapIntegerAttributeToString(value);
            }
        }
    }
}

