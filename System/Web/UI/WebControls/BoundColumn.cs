namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    public class BoundColumn : DataGridColumn
    {
        private string boundField;
        private PropertyDescriptor boundFieldDesc;
        private bool boundFieldDescValid;
        private string formatting;
        public static readonly string thisExpr = "!";

        protected virtual string FormatDataValue(object dataValue)
        {
            string str = string.Empty;
            if (DataBinder.IsNull(dataValue))
            {
                return str;
            }
            if (this.formatting.Length == 0)
            {
                return dataValue.ToString();
            }
            return string.Format(CultureInfo.CurrentCulture, this.formatting, new object[] { dataValue });
        }

        public override void Initialize()
        {
            base.Initialize();
            this.boundFieldDesc = null;
            this.boundFieldDescValid = false;
            this.boundField = this.DataField;
            this.formatting = this.DataFormatString;
        }

        public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
        {
            base.InitializeCell(cell, columnIndex, itemType);
            Control child = null;
            Control control2 = null;
            switch (itemType)
            {
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                case ListItemType.SelectedItem:
                    break;

                case ListItemType.EditItem:
                {
                    if (this.ReadOnly)
                    {
                        break;
                    }
                    TextBox box = new TextBox();
                    child = box;
                    if (this.boundField.Length != 0)
                    {
                        control2 = box;
                    }
                    goto Label_005F;
                }
                default:
                    goto Label_005F;
            }
            if (this.DataField.Length != 0)
            {
                control2 = cell;
            }
        Label_005F:
            if (child != null)
            {
                cell.Controls.Add(child);
            }
            if (control2 != null)
            {
                control2.DataBinding += new EventHandler(this.OnDataBindColumn);
            }
        }

        private void OnDataBindColumn(object sender, EventArgs e)
        {
            string str;
            Control control = (Control) sender;
            DataGridItem namingContainer = (DataGridItem) control.NamingContainer;
            object dataItem = namingContainer.DataItem;
            if (!this.boundFieldDescValid)
            {
                if (!this.boundField.Equals(thisExpr))
                {
                    this.boundFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(this.boundField, true);
                    if ((this.boundFieldDesc == null) && !base.DesignMode)
                    {
                        throw new HttpException(System.Web.SR.GetString("Field_Not_Found", new object[] { this.boundField }));
                    }
                }
                this.boundFieldDescValid = true;
            }
            object dataValue = dataItem;
            if ((this.boundFieldDesc == null) && base.DesignMode)
            {
                str = System.Web.SR.GetString("Sample_Databound_Text");
            }
            else
            {
                if (this.boundFieldDesc != null)
                {
                    dataValue = this.boundFieldDesc.GetValue(dataItem);
                }
                str = this.FormatDataValue(dataValue);
            }
            if (control is TableCell)
            {
                if (str.Length == 0)
                {
                    str = "&nbsp;";
                }
                ((TableCell) control).Text = str;
            }
            else
            {
                ((TextBox) control).Text = str;
            }
        }

        [WebSysDescription("BoundColumn_DataField"), DefaultValue(""), WebCategory("Data")]
        public virtual string DataField
        {
            get
            {
                object obj2 = base.ViewState["DataField"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                base.ViewState["DataField"] = value;
                this.OnColumnChanged();
            }
        }

        [DefaultValue(""), WebCategory("Behavior"), WebSysDescription("BoundColumn_DataFormatString")]
        public virtual string DataFormatString
        {
            get
            {
                object obj2 = base.ViewState["DataFormatString"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                base.ViewState["DataFormatString"] = value;
                this.OnColumnChanged();
            }
        }

        [WebSysDescription("BoundColumn_ReadOnly"), DefaultValue(false), WebCategory("Behavior")]
        public virtual bool ReadOnly
        {
            get
            {
                object obj2 = base.ViewState["ReadOnly"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                base.ViewState["ReadOnly"] = value;
                this.OnColumnChanged();
            }
        }
    }
}

