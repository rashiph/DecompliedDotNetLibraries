namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [ParseChildren(true, "Rows")]
    public class HtmlTable : HtmlContainerControl
    {
        private HtmlTableRowCollection rows;

        public HtmlTable() : base("table")
        {
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new HtmlTableRowControlCollection(this);
        }

        protected internal override void RenderChildren(HtmlTextWriter writer)
        {
            writer.WriteLine();
            writer.Indent++;
            base.RenderChildren(writer);
            writer.Indent--;
        }

        protected override void RenderEndTag(HtmlTextWriter writer)
        {
            base.RenderEndTag(writer);
            writer.WriteLine();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Layout"), DefaultValue("")]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Appearance"), DefaultValue(-1)]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(""), WebCategory("Appearance")]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(""), WebCategory("Appearance")]
        public int CellPadding
        {
            get
            {
                string s = base.Attributes["cellpadding"];
                if (s == null)
                {
                    return -1;
                }
                return int.Parse(s, CultureInfo.InvariantCulture);
            }
            set
            {
                base.Attributes["cellpadding"] = HtmlControl.MapIntegerAttributeToString(value);
            }
        }

        [DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Appearance")]
        public int CellSpacing
        {
            get
            {
                string s = base.Attributes["cellspacing"];
                if (s == null)
                {
                    return -1;
                }
                return int.Parse(s, CultureInfo.InvariantCulture);
            }
            set
            {
                base.Attributes["cellspacing"] = HtmlControl.MapIntegerAttributeToString(value);
            }
        }

        [WebCategory("Layout"), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        public override string InnerHtml
        {
            get
            {
                throw new NotSupportedException(System.Web.SR.GetString("InnerHtml_not_supported", new object[] { base.GetType().Name }));
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("InnerHtml_not_supported", new object[] { base.GetType().Name }));
            }
        }

        public override string InnerText
        {
            get
            {
                throw new NotSupportedException(System.Web.SR.GetString("InnerText_not_supported", new object[] { base.GetType().Name }));
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("InnerText_not_supported", new object[] { base.GetType().Name }));
            }
        }

        [IgnoreUnknownContent, Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual HtmlTableRowCollection Rows
        {
            get
            {
                if (this.rows == null)
                {
                    this.rows = new HtmlTableRowCollection(this);
                }
                return this.rows;
            }
        }

        [DefaultValue(""), WebCategory("Layout"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        protected class HtmlTableRowControlCollection : ControlCollection
        {
            internal HtmlTableRowControlCollection(Control owner) : base(owner)
            {
            }

            public override void Add(Control child)
            {
                if (!(child is HtmlTableRow))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Cannot_Have_Children_Of_Type", new object[] { "HtmlTable", child.GetType().Name.ToString(CultureInfo.InvariantCulture) }));
                }
                base.Add(child);
            }

            public override void AddAt(int index, Control child)
            {
                if (!(child is HtmlTableRow))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Cannot_Have_Children_Of_Type", new object[] { "HtmlTable", child.GetType().Name.ToString(CultureInfo.InvariantCulture) }));
                }
                base.AddAt(index, child);
            }
        }
    }
}

