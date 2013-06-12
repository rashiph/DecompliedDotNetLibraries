namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    public class ButtonField : ButtonFieldBase
    {
        private PropertyDescriptor textFieldDesc;

        protected override void CopyProperties(DataControlField newField)
        {
            ((ButtonField) newField).CommandName = this.CommandName;
            ((ButtonField) newField).DataTextField = this.DataTextField;
            ((ButtonField) newField).DataTextFormatString = this.DataTextFormatString;
            ((ButtonField) newField).ImageUrl = this.ImageUrl;
            ((ButtonField) newField).Text = this.Text;
            base.CopyProperties(newField);
        }

        protected override DataControlField CreateField()
        {
            return new ButtonField();
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

        public override bool Initialize(bool sortingEnabled, Control control)
        {
            base.Initialize(sortingEnabled, control);
            this.textFieldDesc = null;
            return false;
        }

        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            base.InitializeCell(cell, cellType, rowState, rowIndex);
            if ((cellType != DataControlCellType.Header) && (cellType != DataControlCellType.Footer))
            {
                IButtonControl control;
                IPostBackContainer container = base.Control as IPostBackContainer;
                bool causesValidation = this.CausesValidation;
                bool flag2 = true;
                switch (this.ButtonType)
                {
                    case ButtonType.Button:
                        if ((container == null) || causesValidation)
                        {
                            control = new Button();
                        }
                        else
                        {
                            control = new DataControlButton(container);
                            flag2 = false;
                        }
                        break;

                    case ButtonType.Link:
                        if ((container == null) || causesValidation)
                        {
                            control = new DataControlLinkButton(null);
                        }
                        else
                        {
                            control = new DataControlLinkButton(container);
                            flag2 = false;
                        }
                        break;

                    default:
                        if ((container != null) && !causesValidation)
                        {
                            control = new DataControlImageButton(container);
                            flag2 = false;
                        }
                        else
                        {
                            control = new ImageButton();
                        }
                        ((ImageButton) control).ImageUrl = this.ImageUrl;
                        break;
                }
                control.Text = this.Text;
                control.CommandName = this.CommandName;
                control.CommandArgument = rowIndex.ToString(CultureInfo.InvariantCulture);
                if (flag2)
                {
                    control.CausesValidation = causesValidation;
                }
                control.ValidationGroup = this.ValidationGroup;
                if ((this.DataTextField.Length != 0) && base.Visible)
                {
                    ((WebControl) control).DataBinding += new EventHandler(this.OnDataBindField);
                }
                cell.Controls.Add((WebControl) control);
            }
        }

        private void OnDataBindField(object sender, EventArgs e)
        {
            string str;
            Control control = (Control) sender;
            Control namingContainer = control.NamingContainer;
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
            if ((this.textFieldDesc == null) && (component != null))
            {
                string dataTextField = this.DataTextField;
                this.textFieldDesc = TypeDescriptor.GetProperties(component).Find(dataTextField, true);
                if ((this.textFieldDesc == null) && !base.DesignMode)
                {
                    throw new HttpException(System.Web.SR.GetString("Field_Not_Found", new object[] { dataTextField }));
                }
            }
            if ((this.textFieldDesc != null) && (component != null))
            {
                object dataTextValue = this.textFieldDesc.GetValue(component);
                str = this.FormatDataTextValue(dataTextValue);
            }
            else
            {
                str = System.Web.SR.GetString("Sample_Databound_Text");
            }
            ((IButtonControl) control).Text = str;
        }

        public override void ValidateSupportsCallback()
        {
        }

        [WebSysDescription("WebControl_CommandName"), WebCategory("Behavior"), DefaultValue("")]
        public virtual string CommandName
        {
            get
            {
                object obj2 = base.ViewState["CommandName"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["CommandName"]))
                {
                    base.ViewState["CommandName"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebSysDescription("ButtonField_DataTextField"), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebCategory("Data"), DefaultValue("")]
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

        [DefaultValue(""), WebSysDescription("ButtonField_DataTextFormatString"), WebCategory("Data")]
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

        [WebCategory("Appearance"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebSysDescription("ButtonField_ImageUrl")]
        public virtual string ImageUrl
        {
            get
            {
                object obj2 = base.ViewState["ImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["ImageUrl"]))
                {
                    base.ViewState["ImageUrl"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebSysDescription("ButtonField_Text"), Localizable(true), WebCategory("Appearance"), DefaultValue("")]
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

