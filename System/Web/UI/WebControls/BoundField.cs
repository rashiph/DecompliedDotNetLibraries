namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    public class BoundField : DataControlField
    {
        private PropertyDescriptor _boundFieldDesc;
        private string _dataField;
        private string _dataFormatString;
        private bool _htmlEncode;
        private bool _htmlEncodeFormatString;
        private bool _htmlEncodeFormatStringSet;
        private bool _htmlEncodeSet;
        private bool _suppressHeaderTextFieldChange;
        public static readonly string ThisExpression = "!";

        protected override void CopyProperties(DataControlField newField)
        {
            ((BoundField) newField).ApplyFormatInEditMode = this.ApplyFormatInEditMode;
            ((BoundField) newField).ConvertEmptyStringToNull = this.ConvertEmptyStringToNull;
            ((BoundField) newField).DataField = this.DataField;
            ((BoundField) newField).DataFormatString = this.DataFormatString;
            ((BoundField) newField).HtmlEncode = this.HtmlEncode;
            ((BoundField) newField).HtmlEncodeFormatString = this.HtmlEncodeFormatString;
            ((BoundField) newField).NullDisplayText = this.NullDisplayText;
            ((BoundField) newField).ReadOnly = this.ReadOnly;
            base.CopyProperties(newField);
        }

        protected override DataControlField CreateField()
        {
            return new BoundField();
        }

        public override void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
        {
            Control control = null;
            string dataField = this.DataField;
            object text = null;
            string nullDisplayText = this.NullDisplayText;
            if (((rowState & DataControlRowState.Insert) == DataControlRowState.Normal) || this.InsertVisible)
            {
                if (cell.Controls.Count > 0)
                {
                    control = cell.Controls[0];
                    TextBox box = control as TextBox;
                    if (box != null)
                    {
                        text = box.Text;
                    }
                }
                else if (includeReadOnly)
                {
                    string s = cell.Text;
                    if (s == "&nbsp;")
                    {
                        text = string.Empty;
                    }
                    else if (this.SupportsHtmlEncode && this.HtmlEncode)
                    {
                        text = HttpUtility.HtmlDecode(s);
                    }
                    else
                    {
                        text = s;
                    }
                }
                if (text != null)
                {
                    if (((text is string) && (((string) text).Length == 0)) && this.ConvertEmptyStringToNull)
                    {
                        text = null;
                    }
                    if (((text is string) && (((string) text) == nullDisplayText)) && (nullDisplayText.Length > 0))
                    {
                        text = null;
                    }
                    if (dictionary.Contains(dataField))
                    {
                        dictionary[dataField] = text;
                    }
                    else
                    {
                        dictionary.Add(dataField, text);
                    }
                }
            }
        }

        protected virtual string FormatDataValue(object dataValue, bool encode)
        {
            if (!DataBinder.IsNull(dataValue))
            {
                string s = dataValue.ToString();
                string dataFormatString = this.DataFormatString;
                int length = s.Length;
                if (!this.HtmlEncodeFormatString)
                {
                    if ((length > 0) && encode)
                    {
                        s = HttpUtility.HtmlEncode(s);
                    }
                    if ((length == 0) && this.ConvertEmptyStringToNull)
                    {
                        return this.NullDisplayText;
                    }
                    if (dataFormatString.Length == 0)
                    {
                        return s;
                    }
                    if (encode)
                    {
                        return string.Format(CultureInfo.CurrentCulture, dataFormatString, new object[] { s });
                    }
                    return string.Format(CultureInfo.CurrentCulture, dataFormatString, new object[] { dataValue });
                }
                if ((length == 0) && this.ConvertEmptyStringToNull)
                {
                    return this.NullDisplayText;
                }
                if (!string.IsNullOrEmpty(dataFormatString))
                {
                    s = string.Format(CultureInfo.CurrentCulture, dataFormatString, new object[] { dataValue });
                }
                if (!string.IsNullOrEmpty(s) && encode)
                {
                    s = HttpUtility.HtmlEncode(s);
                }
                return s;
            }
            return this.NullDisplayText;
        }

        protected virtual object GetDesignTimeValue()
        {
            return System.Web.SR.GetString("Sample_Databound_Text");
        }

        protected virtual object GetValue(Control controlContainer)
        {
            object component = null;
            string dataField = this.DataField;
            if (controlContainer == null)
            {
                throw new HttpException(System.Web.SR.GetString("DataControlField_NoContainer"));
            }
            component = DataBinder.GetDataItem(controlContainer);
            if ((component == null) && !base.DesignMode)
            {
                throw new HttpException(System.Web.SR.GetString("DataItem_Not_Found"));
            }
            if ((this._boundFieldDesc == null) && !dataField.Equals(ThisExpression))
            {
                this._boundFieldDesc = TypeDescriptor.GetProperties(component).Find(dataField, true);
                if ((this._boundFieldDesc == null) && !base.DesignMode)
                {
                    throw new HttpException(System.Web.SR.GetString("Field_Not_Found", new object[] { dataField }));
                }
            }
            if ((this._boundFieldDesc != null) && (component != null))
            {
                return this._boundFieldDesc.GetValue(component);
            }
            if (base.DesignMode)
            {
                return this.GetDesignTimeValue();
            }
            return component;
        }

        public override bool Initialize(bool enableSorting, Control control)
        {
            base.Initialize(enableSorting, control);
            this._boundFieldDesc = null;
            return false;
        }

        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            string headerText = null;
            bool flag = false;
            bool flag2 = false;
            if (((cellType == DataControlCellType.Header) && this.SupportsHtmlEncode) && this.HtmlEncode)
            {
                headerText = this.HeaderText;
                flag2 = true;
            }
            if (flag2 && !string.IsNullOrEmpty(headerText))
            {
                this._suppressHeaderTextFieldChange = true;
                this.HeaderText = HttpUtility.HtmlEncode(headerText);
                flag = true;
            }
            base.InitializeCell(cell, cellType, rowState, rowIndex);
            if (flag)
            {
                this.HeaderText = headerText;
                this._suppressHeaderTextFieldChange = false;
            }
            if (cellType == DataControlCellType.DataCell)
            {
                this.InitializeDataCell(cell, rowState);
            }
        }

        protected virtual void InitializeDataCell(DataControlFieldCell cell, DataControlRowState rowState)
        {
            Control child = null;
            Control control2 = null;
            if ((((rowState & DataControlRowState.Edit) != DataControlRowState.Normal) && !this.ReadOnly) || ((rowState & DataControlRowState.Insert) != DataControlRowState.Normal))
            {
                TextBox box = new TextBox {
                    ToolTip = this.HeaderText
                };
                child = box;
                if ((this.DataField.Length != 0) && ((rowState & DataControlRowState.Edit) != DataControlRowState.Normal))
                {
                    control2 = box;
                }
            }
            else if (this.DataField.Length != 0)
            {
                control2 = cell;
            }
            if (child != null)
            {
                cell.Controls.Add(child);
            }
            if ((control2 != null) && base.Visible)
            {
                control2.DataBinding += new EventHandler(this.OnDataBindField);
            }
        }

        protected override void LoadViewState(object state)
        {
            this._dataField = null;
            this._dataFormatString = null;
            this._htmlEncodeSet = false;
            this._htmlEncodeFormatStringSet = false;
            base.LoadViewState(state);
        }

        protected virtual void OnDataBindField(object sender, EventArgs e)
        {
            Control control = (Control) sender;
            Control namingContainer = control.NamingContainer;
            object dataValue = this.GetValue(namingContainer);
            bool encode = (this.SupportsHtmlEncode && this.HtmlEncode) && (control is TableCell);
            string str = this.FormatDataValue(dataValue, encode);
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
                if (!(control is TextBox))
                {
                    throw new HttpException(System.Web.SR.GetString("BoundField_WrongControlType", new object[] { this.DataField }));
                }
                if (this.ApplyFormatInEditMode)
                {
                    ((TextBox) control).Text = str;
                }
                else if (dataValue != null)
                {
                    ((TextBox) control).Text = dataValue.ToString();
                }
                if ((dataValue != null) && dataValue.GetType().IsPrimitive)
                {
                    ((TextBox) control).Columns = 5;
                }
            }
        }

        public override void ValidateSupportsCallback()
        {
        }

        [WebSysDescription("BoundField_ApplyFormatInEditMode"), DefaultValue(false), WebCategory("Behavior")]
        public virtual bool ApplyFormatInEditMode
        {
            get
            {
                object obj2 = base.ViewState["ApplyFormatInEditMode"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                base.ViewState["ApplyFormatInEditMode"] = value;
            }
        }

        [WebCategory("Behavior"), WebSysDescription("BoundField_ConvertEmptyStringToNull"), DefaultValue(true)]
        public virtual bool ConvertEmptyStringToNull
        {
            get
            {
                object obj2 = base.ViewState["ConvertEmptyStringToNull"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                base.ViewState["ConvertEmptyStringToNull"] = value;
            }
        }

        [DefaultValue(""), WebCategory("Data"), WebSysDescription("BoundField_DataField"), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public virtual string DataField
        {
            get
            {
                if (this._dataField == null)
                {
                    object obj2 = base.ViewState["DataField"];
                    if (obj2 != null)
                    {
                        this._dataField = (string) obj2;
                    }
                    else
                    {
                        this._dataField = string.Empty;
                    }
                }
                return this._dataField;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["DataField"]))
                {
                    base.ViewState["DataField"] = value;
                    this._dataField = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Data"), DefaultValue(""), WebSysDescription("BoundField_DataFormatString")]
        public virtual string DataFormatString
        {
            get
            {
                if (this._dataFormatString == null)
                {
                    object obj2 = base.ViewState["DataFormatString"];
                    if (obj2 != null)
                    {
                        this._dataFormatString = (string) obj2;
                    }
                    else
                    {
                        this._dataFormatString = string.Empty;
                    }
                }
                return this._dataFormatString;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["DataFormatString"]))
                {
                    base.ViewState["DataFormatString"] = value;
                    this._dataFormatString = value;
                    this.OnFieldChanged();
                }
            }
        }

        public override string HeaderText
        {
            get
            {
                return base.HeaderText;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["HeaderText"]))
                {
                    base.ViewState["HeaderText"] = value;
                    if (!this._suppressHeaderTextFieldChange)
                    {
                        this.OnFieldChanged();
                    }
                }
            }
        }

        [WebSysDescription("BoundField_HtmlEncode"), WebCategory("Behavior"), DefaultValue(true)]
        public virtual bool HtmlEncode
        {
            get
            {
                if (!this._htmlEncodeSet)
                {
                    object obj2 = base.ViewState["HtmlEncode"];
                    if (obj2 != null)
                    {
                        this._htmlEncode = (bool) obj2;
                    }
                    else
                    {
                        this._htmlEncode = true;
                    }
                    this._htmlEncodeSet = true;
                }
                return this._htmlEncode;
            }
            set
            {
                object obj2 = base.ViewState["HtmlEncode"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    base.ViewState["HtmlEncode"] = value;
                    this._htmlEncode = value;
                    this._htmlEncodeSet = true;
                    this.OnFieldChanged();
                }
            }
        }

        [DefaultValue(true), WebCategory("Behavior")]
        public virtual bool HtmlEncodeFormatString
        {
            get
            {
                if (!this._htmlEncodeFormatStringSet)
                {
                    object obj2 = base.ViewState["HtmlEncodeFormatString"];
                    if (obj2 != null)
                    {
                        this._htmlEncodeFormatString = (bool) obj2;
                    }
                    else
                    {
                        this._htmlEncodeFormatString = true;
                    }
                    this._htmlEncodeFormatStringSet = true;
                }
                return this._htmlEncodeFormatString;
            }
            set
            {
                object obj2 = base.ViewState["HtmlEncodeFormatString"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    base.ViewState["HtmlEncodeFormatString"] = value;
                    this._htmlEncodeFormatString = value;
                    this._htmlEncodeFormatStringSet = true;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Behavior"), DefaultValue(""), WebSysDescription("BoundField_NullDisplayText")]
        public virtual string NullDisplayText
        {
            get
            {
                object obj2 = base.ViewState["NullDisplayText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["NullDisplayText"]))
                {
                    base.ViewState["NullDisplayText"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Behavior"), DefaultValue(false), WebSysDescription("BoundField_ReadOnly")]
        public virtual bool ReadOnly
        {
            get
            {
                object obj2 = base.ViewState["ReadOnly"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                object obj2 = base.ViewState["ReadOnly"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    base.ViewState["ReadOnly"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        protected virtual bool SupportsHtmlEncode
        {
            get
            {
                return true;
            }
        }
    }
}

