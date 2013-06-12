namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    public class ButtonColumn : DataGridColumn
    {
        private PropertyDescriptor textFieldDesc;

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

        public override void Initialize()
        {
            base.Initialize();
            this.textFieldDesc = null;
        }

        public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
        {
            base.InitializeCell(cell, columnIndex, itemType);
            if ((itemType != ListItemType.Header) && (itemType != ListItemType.Footer))
            {
                WebControl child = null;
                if (this.ButtonType == ButtonColumnType.LinkButton)
                {
                    LinkButton button = new DataGridLinkButton {
                        Text = this.Text,
                        CommandName = this.CommandName,
                        CausesValidation = this.CausesValidation,
                        ValidationGroup = this.ValidationGroup
                    };
                    child = button;
                }
                else
                {
                    Button button2 = new Button {
                        Text = this.Text,
                        CommandName = this.CommandName,
                        CausesValidation = this.CausesValidation,
                        ValidationGroup = this.ValidationGroup
                    };
                    child = button2;
                }
                if (this.DataTextField.Length != 0)
                {
                    child.DataBinding += new EventHandler(this.OnDataBindColumn);
                }
                cell.Controls.Add(child);
            }
        }

        private void OnDataBindColumn(object sender, EventArgs e)
        {
            string str2;
            Control control = (Control) sender;
            DataGridItem namingContainer = (DataGridItem) control.NamingContainer;
            object dataItem = namingContainer.DataItem;
            if (this.textFieldDesc == null)
            {
                string dataTextField = this.DataTextField;
                this.textFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(dataTextField, true);
                if ((this.textFieldDesc == null) && !base.DesignMode)
                {
                    throw new HttpException(System.Web.SR.GetString("Field_Not_Found", new object[] { dataTextField }));
                }
            }
            if (this.textFieldDesc != null)
            {
                object dataTextValue = this.textFieldDesc.GetValue(dataItem);
                str2 = this.FormatDataTextValue(dataTextValue);
            }
            else
            {
                str2 = System.Web.SR.GetString("Sample_Databound_Text");
            }
            if (control is LinkButton)
            {
                ((LinkButton) control).Text = str2;
            }
            else
            {
                ((Button) control).Text = str2;
            }
        }

        [WebCategory("Appearance"), DefaultValue(0), WebSysDescription("ButtonColumn_ButtonType")]
        public virtual ButtonColumnType ButtonType
        {
            get
            {
                object obj2 = base.ViewState["ButtonType"];
                if (obj2 != null)
                {
                    return (ButtonColumnType) obj2;
                }
                return ButtonColumnType.LinkButton;
            }
            set
            {
                if ((value < ButtonColumnType.LinkButton) || (value > ButtonColumnType.PushButton))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["ButtonType"] = value;
                this.OnColumnChanged();
            }
        }

        [WebSysDescription("ButtonColumn_CausesValidation"), DefaultValue(false)]
        public virtual bool CausesValidation
        {
            get
            {
                object obj2 = base.ViewState["CausesValidation"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                base.ViewState["CausesValidation"] = value;
                this.OnColumnChanged();
            }
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
                base.ViewState["CommandName"] = value;
                this.OnColumnChanged();
            }
        }

        [WebCategory("Data"), WebSysDescription("ButtonColumn_DataTextField"), DefaultValue("")]
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
                base.ViewState["DataTextField"] = value;
                this.OnColumnChanged();
            }
        }

        [WebSysDescription("ButtonColumn_DataTextFormatString"), WebCategory("Data"), DefaultValue("")]
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
                base.ViewState["DataTextFormatString"] = value;
                this.OnColumnChanged();
            }
        }

        [WebCategory("Appearance"), Localizable(true), DefaultValue(""), WebSysDescription("ButtonColumn_Text")]
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
                base.ViewState["Text"] = value;
                this.OnColumnChanged();
            }
        }

        [DefaultValue(""), WebSysDescription("ButtonColumn_ValidationGroup")]
        public virtual string ValidationGroup
        {
            get
            {
                object obj2 = base.ViewState["ValidationGroup"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                base.ViewState["ValidationGroup"] = value;
                this.OnColumnChanged();
            }
        }
    }
}

