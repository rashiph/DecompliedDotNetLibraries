namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    public class HyperLinkField : DataControlField
    {
        private PropertyDescriptor textFieldDesc;
        private PropertyDescriptor[] urlFieldDescs;

        protected override void CopyProperties(DataControlField newField)
        {
            ((HyperLinkField) newField).DataNavigateUrlFields = this.DataNavigateUrlFields;
            ((HyperLinkField) newField).DataNavigateUrlFormatString = this.DataNavigateUrlFormatString;
            ((HyperLinkField) newField).DataTextField = this.DataTextField;
            ((HyperLinkField) newField).DataTextFormatString = this.DataTextFormatString;
            ((HyperLinkField) newField).NavigateUrl = this.NavigateUrl;
            ((HyperLinkField) newField).Target = this.Target;
            ((HyperLinkField) newField).Text = this.Text;
            base.CopyProperties(newField);
        }

        protected override DataControlField CreateField()
        {
            return new HyperLinkField();
        }

        protected virtual string FormatDataNavigateUrlValue(object[] dataUrlValues)
        {
            string str = string.Empty;
            if (dataUrlValues == null)
            {
                return str;
            }
            string dataNavigateUrlFormatString = this.DataNavigateUrlFormatString;
            if (dataNavigateUrlFormatString.Length == 0)
            {
                if ((dataUrlValues.Length > 0) && !DataBinder.IsNull(dataUrlValues[0]))
                {
                    str = dataUrlValues[0].ToString();
                }
                return str;
            }
            return string.Format(CultureInfo.CurrentCulture, dataNavigateUrlFormatString, dataUrlValues);
        }

        protected virtual string FormatDataTextValue(object dataTextValue)
        {
            string str = string.Empty;
            if (DataBinder.IsNull(dataTextValue))
            {
                return str;
            }
            string dataTextFormatString = this.DataTextFormatString;
            if (dataTextFormatString.Length == 0)
            {
                return dataTextValue.ToString();
            }
            return string.Format(CultureInfo.CurrentCulture, dataTextFormatString, new object[] { dataTextValue });
        }

        public override bool Initialize(bool enableSorting, Control control)
        {
            base.Initialize(enableSorting, control);
            this.textFieldDesc = null;
            this.urlFieldDescs = null;
            return false;
        }

        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            base.InitializeCell(cell, cellType, rowState, rowIndex);
            if (cellType == DataControlCellType.DataCell)
            {
                HyperLink child = new HyperLink {
                    Text = this.Text,
                    NavigateUrl = this.NavigateUrl,
                    Target = this.Target
                };
                if (((rowState & DataControlRowState.Insert) == DataControlRowState.Normal) && base.Visible)
                {
                    if ((this.DataNavigateUrlFields.Length != 0) || (this.DataTextField.Length != 0))
                    {
                        child.DataBinding += new EventHandler(this.OnDataBindField);
                    }
                    cell.Controls.Add(child);
                }
            }
        }

        private void OnDataBindField(object sender, EventArgs e)
        {
            HyperLink link = (HyperLink) sender;
            Control namingContainer = link.NamingContainer;
            object component = null;
            if (namingContainer == null)
            {
                throw new HttpException(System.Web.SR.GetString("DataControlField_NoContainer"));
            }
            component = DataBinder.GetDataItem(namingContainer);
            if ((component == null) && !base.DesignMode)
            {
                throw new HttpException(System.Web.SR.GetString("DataItem_Not_Found"));
            }
            if ((this.textFieldDesc == null) && (this.urlFieldDescs == null))
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
                string dataTextField = this.DataTextField;
                if (dataTextField.Length != 0)
                {
                    this.textFieldDesc = properties.Find(dataTextField, true);
                    if ((this.textFieldDesc == null) && !base.DesignMode)
                    {
                        throw new HttpException(System.Web.SR.GetString("Field_Not_Found", new object[] { dataTextField }));
                    }
                }
                string[] dataNavigateUrlFields = this.DataNavigateUrlFields;
                int num = dataNavigateUrlFields.Length;
                this.urlFieldDescs = new PropertyDescriptor[num];
                for (int i = 0; i < num; i++)
                {
                    dataTextField = dataNavigateUrlFields[i];
                    if (dataTextField.Length != 0)
                    {
                        this.urlFieldDescs[i] = properties.Find(dataTextField, true);
                        if ((this.urlFieldDescs[i] == null) && !base.DesignMode)
                        {
                            throw new HttpException(System.Web.SR.GetString("Field_Not_Found", new object[] { dataTextField }));
                        }
                    }
                }
            }
            string str2 = string.Empty;
            if ((this.textFieldDesc != null) && (component != null))
            {
                object dataTextValue = this.textFieldDesc.GetValue(component);
                str2 = this.FormatDataTextValue(dataTextValue);
            }
            if ((base.DesignMode && (this.DataTextField.Length != 0)) && (str2.Length == 0))
            {
                str2 = System.Web.SR.GetString("Sample_Databound_Text");
            }
            if (str2.Length > 0)
            {
                link.Text = str2;
            }
            int length = this.urlFieldDescs.Length;
            string str3 = string.Empty;
            if (((this.urlFieldDescs != null) && (length > 0)) && (component != null))
            {
                object[] dataUrlValues = new object[length];
                for (int j = 0; j < length; j++)
                {
                    if (this.urlFieldDescs[j] != null)
                    {
                        dataUrlValues[j] = this.urlFieldDescs[j].GetValue(component);
                    }
                }
                string s = this.FormatDataNavigateUrlValue(dataUrlValues);
                if (!CrossSiteScriptingValidation.IsDangerousUrl(s))
                {
                    str3 = s;
                }
            }
            if ((base.DesignMode && (this.DataNavigateUrlFields.Length != 0)) && (str3.Length == 0))
            {
                str3 = "url";
            }
            if (str3.Length > 0)
            {
                link.NavigateUrl = str3;
            }
        }

        private bool StringArraysEqual(string[] arr1, string[] arr2)
        {
            if ((arr1 != null) || (arr2 != null))
            {
                if ((arr1 == null) || (arr2 == null))
                {
                    return false;
                }
                if (arr1.Length != arr2.Length)
                {
                    return false;
                }
                for (int i = 0; i < arr1.Length; i++)
                {
                    if (!string.Equals(arr1[i], arr2[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override void ValidateSupportsCallback()
        {
        }

        [TypeConverter(typeof(StringArrayConverter)), WebSysDescription("HyperLinkField_DataNavigateUrlFields"), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.DataFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Data")]
        public virtual string[] DataNavigateUrlFields
        {
            get
            {
                object obj2 = base.ViewState["DataNavigateUrlFields"];
                if (obj2 != null)
                {
                    return (string[]) ((string[]) obj2).Clone();
                }
                return new string[0];
            }
            set
            {
                string[] strArray = base.ViewState["DataNavigateUrlFields"] as string[];
                if (!this.StringArraysEqual(strArray, value))
                {
                    if (value != null)
                    {
                        base.ViewState["DataNavigateUrlFields"] = (string[]) value.Clone();
                    }
                    else
                    {
                        base.ViewState["DataNavigateUrlFields"] = null;
                    }
                    this.OnFieldChanged();
                }
            }
        }

        [DefaultValue(""), WebCategory("Data"), WebSysDescription("HyperLinkField_DataNavigateUrlFormatString")]
        public virtual string DataNavigateUrlFormatString
        {
            get
            {
                object obj2 = base.ViewState["DataNavigateUrlFormatString"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["DataNavigateUrlFormatString"]))
                {
                    base.ViewState["DataNavigateUrlFormatString"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [DefaultValue(""), WebCategory("Data"), WebSysDescription("HyperLinkField_DataTextField"), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public virtual string DataTextField
        {
            get
            {
                object obj2 = base.ViewState["DataTextField"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["DataTextField"]))
                {
                    base.ViewState["DataTextField"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebSysDescription("HyperLinkField_DataTextFormatString"), WebCategory("Data"), DefaultValue("")]
        public virtual string DataTextFormatString
        {
            get
            {
                object obj2 = base.ViewState["DataTextFormatString"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["DataTextFormatString"]))
                {
                    base.ViewState["DataTextFormatString"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [DefaultValue(""), WebCategory("Behavior"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebSysDescription("HyperLinkField_NavigateUrl")]
        public virtual string NavigateUrl
        {
            get
            {
                object obj2 = base.ViewState["NavigateUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["NavigateUrl"]))
                {
                    base.ViewState["NavigateUrl"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Behavior"), DefaultValue(""), TypeConverter(typeof(TargetConverter)), WebSysDescription("HyperLink_Target")]
        public virtual string Target
        {
            get
            {
                object obj2 = base.ViewState["Target"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["Target"]))
                {
                    base.ViewState["Target"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [Localizable(true), WebSysDescription("HyperLinkField_Text"), WebCategory("Appearance"), DefaultValue("")]
        public virtual string Text
        {
            get
            {
                object obj2 = base.ViewState["Text"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["Text"]))
                {
                    base.ViewState["Text"] = value;
                    this.OnFieldChanged();
                }
            }
        }
    }
}

