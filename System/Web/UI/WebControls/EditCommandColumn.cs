namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;

    public class EditCommandColumn : DataGridColumn
    {
        private void AddButtonToCell(TableCell cell, string commandName, string buttonText, bool causesValidation, string validationGroup)
        {
            WebControl child = null;
            ControlCollection controls = cell.Controls;
            if (this.ButtonType == ButtonColumnType.LinkButton)
            {
                LinkButton button = new DataGridLinkButton();
                child = button;
                button.CommandName = commandName;
                button.Text = buttonText;
                button.CausesValidation = causesValidation;
                button.ValidationGroup = validationGroup;
            }
            else
            {
                Button button2 = new Button();
                child = button2;
                button2.CommandName = commandName;
                button2.Text = buttonText;
                button2.CausesValidation = causesValidation;
                button2.ValidationGroup = validationGroup;
            }
            controls.Add(child);
        }

        public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
        {
            base.InitializeCell(cell, columnIndex, itemType);
            bool causesValidation = this.CausesValidation;
            if ((itemType != ListItemType.Header) && (itemType != ListItemType.Footer))
            {
                if (itemType == ListItemType.EditItem)
                {
                    ControlCollection controls = cell.Controls;
                    this.AddButtonToCell(cell, "Update", this.UpdateText, causesValidation, this.ValidationGroup);
                    LiteralControl child = new LiteralControl("&nbsp;");
                    controls.Add(child);
                    this.AddButtonToCell(cell, "Cancel", this.CancelText, false, string.Empty);
                }
                else
                {
                    this.AddButtonToCell(cell, "Edit", this.EditText, false, string.Empty);
                }
            }
        }

        [DefaultValue(0)]
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

        [DefaultValue(""), Localizable(true)]
        public virtual string CancelText
        {
            get
            {
                object obj2 = base.ViewState["CancelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                base.ViewState["CancelText"] = value;
                this.OnColumnChanged();
            }
        }

        [DefaultValue(true)]
        public virtual bool CausesValidation
        {
            get
            {
                object obj2 = base.ViewState["CausesValidation"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                base.ViewState["CausesValidation"] = value;
                this.OnColumnChanged();
            }
        }

        [DefaultValue(""), Localizable(true)]
        public virtual string EditText
        {
            get
            {
                object obj2 = base.ViewState["EditText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                base.ViewState["EditText"] = value;
                this.OnColumnChanged();
            }
        }

        [DefaultValue(""), Localizable(true)]
        public virtual string UpdateText
        {
            get
            {
                object obj2 = base.ViewState["UpdateText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                base.ViewState["UpdateText"] = value;
                this.OnColumnChanged();
            }
        }

        [DefaultValue("")]
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

