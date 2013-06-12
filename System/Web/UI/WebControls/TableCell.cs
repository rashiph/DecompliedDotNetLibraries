namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [DefaultProperty("Text"), Bindable(false), Designer("System.Web.UI.Design.WebControls.PreviewControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ControlBuilder(typeof(TableCellControlBuilder)), ParseChildren(false), ToolboxItem(false)]
    public class TableCell : WebControl
    {
        private bool _textSetByAddParsedSubObject;

        public TableCell() : this(HtmlTextWriterTag.Td)
        {
        }

        internal TableCell(HtmlTextWriterTag tagKey) : base(tagKey)
        {
            base.PreventAutoID();
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            int columnSpan = this.ColumnSpan;
            if (columnSpan > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Colspan, columnSpan.ToString(NumberFormatInfo.InvariantInfo));
            }
            columnSpan = this.RowSpan;
            if (columnSpan > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Rowspan, columnSpan.ToString(NumberFormatInfo.InvariantInfo));
            }
            string[] associatedHeaderCellID = this.AssociatedHeaderCellID;
            if (associatedHeaderCellID.Length > 0)
            {
                bool flag = true;
                StringBuilder builder = new StringBuilder();
                Control namingContainer = this.NamingContainer;
                foreach (string str in associatedHeaderCellID)
                {
                    TableHeaderCell cell = namingContainer.FindControl(str) as TableHeaderCell;
                    if (cell == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("TableCell_AssociatedHeaderCellNotFound", new object[] { str }));
                    }
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        builder.Append(" ");
                    }
                    builder.Append(cell.ClientID);
                }
                string str2 = builder.ToString();
                if (!string.IsNullOrEmpty(str2))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Headers, str2);
                }
            }
        }

        protected override void AddParsedSubObject(object obj)
        {
            if (this.HasControls())
            {
                base.AddParsedSubObject(obj);
            }
            else if (obj is LiteralControl)
            {
                if (this._textSetByAddParsedSubObject)
                {
                    this.Text = this.Text + ((LiteralControl) obj).Text;
                }
                else
                {
                    this.Text = ((LiteralControl) obj).Text;
                }
                this._textSetByAddParsedSubObject = true;
            }
            else
            {
                string text = this.Text;
                if (text.Length != 0)
                {
                    this.Text = string.Empty;
                    base.AddParsedSubObject(new LiteralControl(text));
                }
                base.AddParsedSubObject(obj);
            }
        }

        protected override Style CreateControlStyle()
        {
            return new TableItemStyle(this.ViewState);
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            if (base.HasRenderingData())
            {
                base.RenderContents(writer);
            }
            else
            {
                writer.Write(this.Text);
            }
        }

        [DefaultValue((string) null), TypeConverter(typeof(StringArrayConverter)), WebCategory("Accessibility"), WebSysDescription("TableCell_AssociatedHeaderCellID")]
        public virtual string[] AssociatedHeaderCellID
        {
            get
            {
                object obj2 = this.ViewState["AssociatedHeaderCellID"];
                if (obj2 == null)
                {
                    return new string[0];
                }
                return (string[]) ((string[]) obj2).Clone();
            }
            set
            {
                if (value != null)
                {
                    this.ViewState["AssociatedHeaderCellID"] = (string[]) value.Clone();
                }
                else
                {
                    this.ViewState["AssociatedHeaderCellID"] = null;
                }
            }
        }

        [WebSysDescription("TableCell_ColumnSpan"), WebCategory("Appearance"), DefaultValue(0)]
        public virtual int ColumnSpan
        {
            get
            {
                object obj2 = this.ViewState["ColumnSpan"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 0;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["ColumnSpan"] = value;
            }
        }

        [WebCategory("Layout"), WebSysDescription("TableItem_HorizontalAlign"), DefaultValue(0)]
        public virtual System.Web.UI.WebControls.HorizontalAlign HorizontalAlign
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return System.Web.UI.WebControls.HorizontalAlign.NotSet;
                }
                return ((TableItemStyle) base.ControlStyle).HorizontalAlign;
            }
            set
            {
                ((TableItemStyle) base.ControlStyle).HorizontalAlign = value;
            }
        }

        [WebCategory("Layout"), DefaultValue(0), WebSysDescription("TableCell_RowSpan")]
        public virtual int RowSpan
        {
            get
            {
                object obj2 = this.ViewState["RowSpan"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 0;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["RowSpan"] = value;
            }
        }

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return (this.RenderingCompatibility < VersionUtil.Framework40);
            }
        }

        [WebSysDescription("TableCell_Text"), DefaultValue(""), PersistenceMode(PersistenceMode.InnerDefaultProperty), Localizable(true), WebCategory("Appearance")]
        public virtual string Text
        {
            get
            {
                object obj2 = this.ViewState["Text"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (this.HasControls())
                {
                    this.Controls.Clear();
                }
                this.ViewState["Text"] = value;
            }
        }

        [WebCategory("Layout"), WebSysDescription("TableItem_VerticalAlign"), DefaultValue(0)]
        public virtual System.Web.UI.WebControls.VerticalAlign VerticalAlign
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return System.Web.UI.WebControls.VerticalAlign.NotSet;
                }
                return ((TableItemStyle) base.ControlStyle).VerticalAlign;
            }
            set
            {
                ((TableItemStyle) base.ControlStyle).VerticalAlign = value;
            }
        }

        [WebSysDescription("TableCell_Wrap"), DefaultValue(true), WebCategory("Layout")]
        public virtual bool Wrap
        {
            get
            {
                return (!base.ControlStyleCreated || ((TableItemStyle) base.ControlStyle).Wrap);
            }
            set
            {
                ((TableItemStyle) base.ControlStyle).Wrap = value;
            }
        }
    }
}

