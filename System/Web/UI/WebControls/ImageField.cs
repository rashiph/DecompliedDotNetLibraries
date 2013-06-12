namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    public class ImageField : DataControlField
    {
        private PropertyDescriptor _altTextFieldDesc;
        private string _dataField;
        private PropertyDescriptor _imageFieldDesc;
        public static readonly string ThisExpression = "!";

        protected override void CopyProperties(DataControlField newField)
        {
            ((ImageField) newField).AlternateText = this.AlternateText;
            ((ImageField) newField).ConvertEmptyStringToNull = this.ConvertEmptyStringToNull;
            ((ImageField) newField).DataAlternateTextField = this.DataAlternateTextField;
            ((ImageField) newField).DataAlternateTextFormatString = this.DataAlternateTextFormatString;
            ((ImageField) newField).DataImageUrlField = this.DataImageUrlField;
            ((ImageField) newField).DataImageUrlFormatString = this.DataImageUrlFormatString;
            ((ImageField) newField).NullDisplayText = this.NullDisplayText;
            ((ImageField) newField).NullImageUrl = this.NullImageUrl;
            ((ImageField) newField).ReadOnly = this.ReadOnly;
            base.CopyProperties(newField);
        }

        protected override DataControlField CreateField()
        {
            return new ImageField();
        }

        public override void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
        {
            Control control = null;
            string dataImageUrlField = this.DataImageUrlField;
            object imageUrl = null;
            bool flag = false;
            if ((((rowState & DataControlRowState.Insert) == DataControlRowState.Normal) || this.InsertVisible) && (cell.Controls.Count != 0))
            {
                control = cell.Controls[0];
                Image image = control as Image;
                if (image != null)
                {
                    if (includeReadOnly)
                    {
                        flag = true;
                        if (image.Visible)
                        {
                            imageUrl = image.ImageUrl;
                        }
                    }
                }
                else
                {
                    TextBox box = control as TextBox;
                    if (box != null)
                    {
                        imageUrl = box.Text;
                        flag = true;
                    }
                }
                if ((imageUrl != null) || flag)
                {
                    if ((this.ConvertEmptyStringToNull && (imageUrl is string)) && (((string) imageUrl).Length == 0))
                    {
                        imageUrl = null;
                    }
                    if (dictionary.Contains(dataImageUrlField))
                    {
                        dictionary[dataImageUrlField] = imageUrl;
                    }
                    else
                    {
                        dictionary.Add(dataImageUrlField, imageUrl);
                    }
                }
            }
        }

        protected virtual string FormatImageUrlValue(object dataValue)
        {
            string str = string.Empty;
            string dataImageUrlFormatString = this.DataImageUrlFormatString;
            if (DataBinder.IsNull(dataValue))
            {
                return null;
            }
            string str3 = dataValue.ToString();
            if (str3.Length <= 0)
            {
                return str;
            }
            if (dataImageUrlFormatString.Length == 0)
            {
                return str3;
            }
            return string.Format(CultureInfo.CurrentCulture, dataImageUrlFormatString, new object[] { dataValue });
        }

        protected virtual string GetDesignTimeValue()
        {
            return System.Web.SR.GetString("Sample_Databound_Text");
        }

        protected virtual string GetFormattedAlternateText(Control controlContainer)
        {
            string dataAlternateTextField = this.DataAlternateTextField;
            string dataAlternateTextFormatString = this.DataAlternateTextFormatString;
            if (dataAlternateTextField.Length > 0)
            {
                object obj2 = this.GetValue(controlContainer, dataAlternateTextField, ref this._altTextFieldDesc);
                string str4 = string.Empty;
                if (!DataBinder.IsNull(obj2))
                {
                    str4 = obj2.ToString();
                }
                if (dataAlternateTextFormatString.Length > 0)
                {
                    return string.Format(CultureInfo.CurrentCulture, dataAlternateTextFormatString, new object[] { obj2 });
                }
                return str4;
            }
            return this.AlternateText;
        }

        protected virtual object GetValue(Control controlContainer, string fieldName, ref PropertyDescriptor cachedDescriptor)
        {
            object obj2 = null;
            object component = null;
            if (controlContainer == null)
            {
                throw new HttpException(System.Web.SR.GetString("DataControlField_NoContainer"));
            }
            component = DataBinder.GetDataItem(controlContainer);
            if ((component == null) && !base.DesignMode)
            {
                throw new HttpException(System.Web.SR.GetString("DataItem_Not_Found"));
            }
            if ((cachedDescriptor == null) && !fieldName.Equals(ThisExpression))
            {
                cachedDescriptor = TypeDescriptor.GetProperties(component).Find(fieldName, true);
                if ((cachedDescriptor == null) && !base.DesignMode)
                {
                    throw new HttpException(System.Web.SR.GetString("Field_Not_Found", new object[] { fieldName }));
                }
            }
            if ((cachedDescriptor != null) && (component != null))
            {
                return cachedDescriptor.GetValue(component);
            }
            if (!base.DesignMode)
            {
                obj2 = component;
            }
            return obj2;
        }

        public override bool Initialize(bool enableSorting, Control control)
        {
            base.Initialize(enableSorting, control);
            this._imageFieldDesc = null;
            this._altTextFieldDesc = null;
            return false;
        }

        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            base.InitializeCell(cell, cellType, rowState, rowIndex);
            if (cellType == DataControlCellType.DataCell)
            {
                this.InitializeDataCell(cell, rowState);
            }
        }

        protected virtual void InitializeDataCell(DataControlFieldCell cell, DataControlRowState rowState)
        {
            Control control = null;
            if ((((rowState & DataControlRowState.Edit) != DataControlRowState.Normal) && !this.ReadOnly) || ((rowState & DataControlRowState.Insert) != DataControlRowState.Normal))
            {
                TextBox child = new TextBox();
                cell.Controls.Add(child);
                if ((this.DataImageUrlField.Length != 0) && ((rowState & DataControlRowState.Edit) != DataControlRowState.Normal))
                {
                    control = child;
                }
            }
            else if (this.DataImageUrlField.Length != 0)
            {
                control = cell;
                Image image = new Image();
                Label label = new Label();
                cell.Controls.Add(image);
                cell.Controls.Add(label);
            }
            if ((control != null) && base.Visible)
            {
                control.DataBinding += new EventHandler(this.OnDataBindField);
            }
        }

        protected virtual void OnDataBindField(object sender, EventArgs e)
        {
            Control control = (Control) sender;
            Control namingContainer = control.NamingContainer;
            string s = null;
            string nullImageUrl = this.NullImageUrl;
            string formattedAlternateText = this.GetFormattedAlternateText(namingContainer);
            if (base.DesignMode && (control is TableCell))
            {
                if ((control.Controls.Count == 0) || !(control.Controls[0] is Image))
                {
                    throw new HttpException(System.Web.SR.GetString("ImageField_WrongControlType", new object[] { this.DataImageUrlField }));
                }
                ((Image) control.Controls[0]).Visible = false;
                ((TableCell) control).Text = this.GetDesignTimeValue();
            }
            else
            {
                object dataValue = this.GetValue(namingContainer, this.DataImageUrlField, ref this._imageFieldDesc);
                s = this.FormatImageUrlValue(dataValue);
                if (control is TableCell)
                {
                    TableCell cell = (TableCell) control;
                    if (((cell.Controls.Count < 2) || !(cell.Controls[0] is Image)) || !(cell.Controls[1] is Label))
                    {
                        throw new HttpException(System.Web.SR.GetString("ImageField_WrongControlType", new object[] { this.DataImageUrlField }));
                    }
                    Image image = (Image) cell.Controls[0];
                    Label label = (Label) cell.Controls[1];
                    label.Visible = false;
                    if ((s == null) || (this.ConvertEmptyStringToNull && (s.Length == 0)))
                    {
                        if (nullImageUrl.Length > 0)
                        {
                            s = nullImageUrl;
                        }
                        else
                        {
                            image.Visible = false;
                            label.Text = this.NullDisplayText;
                            label.Visible = true;
                        }
                    }
                    if (!CrossSiteScriptingValidation.IsDangerousUrl(s))
                    {
                        image.ImageUrl = s;
                    }
                    image.AlternateText = formattedAlternateText;
                }
                else
                {
                    if (!(control is TextBox))
                    {
                        throw new HttpException(System.Web.SR.GetString("ImageField_WrongControlType", new object[] { this.DataImageUrlField }));
                    }
                    ((TextBox) control).Text = dataValue.ToString();
                    ((TextBox) control).ToolTip = formattedAlternateText;
                    if ((dataValue != null) && dataValue.GetType().IsPrimitive)
                    {
                        ((TextBox) control).Columns = 5;
                    }
                }
            }
        }

        public override void ValidateSupportsCallback()
        {
        }

        [Localizable(true), DefaultValue(""), WebCategory("Appearance"), WebSysDescription("ImageField_AlternateText")]
        public virtual string AlternateText
        {
            get
            {
                object obj2 = base.ViewState["AlternateText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["AlternateText"]))
                {
                    base.ViewState["AlternateText"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebSysDescription("ImageField_ConvertEmptyStringToNull"), WebCategory("Behavior"), DefaultValue(true)]
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

        [TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebCategory("Data"), DefaultValue(""), WebSysDescription("ImageField_DataAlternateTextField")]
        public virtual string DataAlternateTextField
        {
            get
            {
                object obj2 = base.ViewState["DataAlternateTextField"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["DataAlternateTextField"]))
                {
                    base.ViewState["DataAlternateTextField"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Data"), DefaultValue(""), WebSysDescription("ImageField_DataAlternateTextFormatString")]
        public virtual string DataAlternateTextFormatString
        {
            get
            {
                object obj2 = base.ViewState["DataAlternateTextFormatString"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["DataAlternateTextFormatString"]))
                {
                    base.ViewState["DataAlternateTextFormatString"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue(""), WebCategory("Data"), WebSysDescription("ImageField_ImageUrlField")]
        public virtual string DataImageUrlField
        {
            get
            {
                if (this._dataField == null)
                {
                    object obj2 = base.ViewState["DataImageUrlField"];
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
                if (!object.Equals(value, base.ViewState["DataImageUrlField"]))
                {
                    base.ViewState["DataImageUrlField"] = value;
                    this._dataField = value;
                    this.OnFieldChanged();
                }
            }
        }

        [DefaultValue(""), WebCategory("Data"), WebSysDescription("ImageField_ImageUrlFormatString")]
        public virtual string DataImageUrlFormatString
        {
            get
            {
                object obj2 = base.ViewState["DataImageUrlFormatString"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["DataImageUrlFormatString"]))
                {
                    base.ViewState["DataImageUrlFormatString"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [Localizable(true), WebCategory("Behavior"), DefaultValue(""), WebSysDescription("BoundField_NullDisplayText")]
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

        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Behavior"), DefaultValue(""), UrlProperty, WebSysDescription("ImageField_NullImageUrl")]
        public virtual string NullImageUrl
        {
            get
            {
                object obj2 = base.ViewState["NullImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["NullImageUrl"]))
                {
                    base.ViewState["NullImageUrl"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Behavior"), DefaultValue(false), WebSysDescription("ImageField_ReadOnly")]
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
    }
}

