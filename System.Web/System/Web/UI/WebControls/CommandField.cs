namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    public class CommandField : ButtonFieldBase
    {
        private void AddButtonToCell(DataControlFieldCell cell, string commandName, string buttonText, bool causesValidation, string validationGroup, int rowIndex, string imageUrl)
        {
            IButtonControl control;
            IPostBackContainer container = base.Control as IPostBackContainer;
            bool flag = true;
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
                        flag = false;
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
                        flag = false;
                    }
                    break;

                default:
                    if ((container != null) && !causesValidation)
                    {
                        control = new DataControlImageButton(container);
                        flag = false;
                    }
                    else
                    {
                        control = new ImageButton();
                    }
                    ((ImageButton) control).ImageUrl = imageUrl;
                    break;
            }
            control.Text = buttonText;
            control.CommandName = commandName;
            control.CommandArgument = rowIndex.ToString(CultureInfo.InvariantCulture);
            if (flag)
            {
                control.CausesValidation = causesValidation;
            }
            control.ValidationGroup = validationGroup;
            cell.Controls.Add((WebControl) control);
        }

        protected override void CopyProperties(DataControlField newField)
        {
            ((CommandField) newField).CancelImageUrl = this.CancelImageUrl;
            ((CommandField) newField).CancelText = this.CancelText;
            ((CommandField) newField).DeleteImageUrl = this.DeleteImageUrl;
            ((CommandField) newField).DeleteText = this.DeleteText;
            ((CommandField) newField).EditImageUrl = this.EditImageUrl;
            ((CommandField) newField).EditText = this.EditText;
            ((CommandField) newField).InsertImageUrl = this.InsertImageUrl;
            ((CommandField) newField).InsertText = this.InsertText;
            ((CommandField) newField).NewImageUrl = this.NewImageUrl;
            ((CommandField) newField).NewText = this.NewText;
            ((CommandField) newField).SelectImageUrl = this.SelectImageUrl;
            ((CommandField) newField).SelectText = this.SelectText;
            ((CommandField) newField).UpdateImageUrl = this.UpdateImageUrl;
            ((CommandField) newField).UpdateText = this.UpdateText;
            ((CommandField) newField).ShowCancelButton = this.ShowCancelButton;
            ((CommandField) newField).ShowDeleteButton = this.ShowDeleteButton;
            ((CommandField) newField).ShowEditButton = this.ShowEditButton;
            ((CommandField) newField).ShowSelectButton = this.ShowSelectButton;
            ((CommandField) newField).ShowInsertButton = this.ShowInsertButton;
            base.CopyProperties(newField);
        }

        protected override DataControlField CreateField()
        {
            return new CommandField();
        }

        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            base.InitializeCell(cell, cellType, rowState, rowIndex);
            bool showEditButton = this.ShowEditButton;
            bool showDeleteButton = this.ShowDeleteButton;
            bool showInsertButton = this.ShowInsertButton;
            bool showSelectButton = this.ShowSelectButton;
            bool showCancelButton = this.ShowCancelButton;
            bool flag6 = true;
            bool causesValidation = this.CausesValidation;
            string validationGroup = this.ValidationGroup;
            if (cellType == DataControlCellType.DataCell)
            {
                LiteralControl control;
                if ((rowState & (DataControlRowState.Insert | DataControlRowState.Edit)) != DataControlRowState.Normal)
                {
                    if (((rowState & DataControlRowState.Edit) != DataControlRowState.Normal) && showEditButton)
                    {
                        this.AddButtonToCell(cell, "Update", this.UpdateText, causesValidation, validationGroup, rowIndex, this.UpdateImageUrl);
                        if (showCancelButton)
                        {
                            control = new LiteralControl("&nbsp;");
                            cell.Controls.Add(control);
                            this.AddButtonToCell(cell, "Cancel", this.CancelText, false, string.Empty, rowIndex, this.CancelImageUrl);
                        }
                    }
                    if (((rowState & DataControlRowState.Insert) != DataControlRowState.Normal) && showInsertButton)
                    {
                        this.AddButtonToCell(cell, "Insert", this.InsertText, causesValidation, validationGroup, rowIndex, this.InsertImageUrl);
                        if (showCancelButton)
                        {
                            control = new LiteralControl("&nbsp;");
                            cell.Controls.Add(control);
                            this.AddButtonToCell(cell, "Cancel", this.CancelText, false, string.Empty, rowIndex, this.CancelImageUrl);
                        }
                    }
                }
                else
                {
                    if (showEditButton)
                    {
                        this.AddButtonToCell(cell, "Edit", this.EditText, false, string.Empty, rowIndex, this.EditImageUrl);
                        flag6 = false;
                    }
                    if (showDeleteButton)
                    {
                        if (!flag6)
                        {
                            control = new LiteralControl("&nbsp;");
                            cell.Controls.Add(control);
                        }
                        this.AddButtonToCell(cell, "Delete", this.DeleteText, false, string.Empty, rowIndex, this.DeleteImageUrl);
                        flag6 = false;
                    }
                    if (showInsertButton)
                    {
                        if (!flag6)
                        {
                            control = new LiteralControl("&nbsp;");
                            cell.Controls.Add(control);
                        }
                        this.AddButtonToCell(cell, "New", this.NewText, false, string.Empty, rowIndex, this.NewImageUrl);
                        flag6 = false;
                    }
                    if (showSelectButton)
                    {
                        if (!flag6)
                        {
                            control = new LiteralControl("&nbsp;");
                            cell.Controls.Add(control);
                        }
                        this.AddButtonToCell(cell, "Select", this.SelectText, false, string.Empty, rowIndex, this.SelectImageUrl);
                        flag6 = false;
                    }
                }
            }
        }

        public override void ValidateSupportsCallback()
        {
            if (this.ShowSelectButton)
            {
                throw new NotSupportedException(System.Web.SR.GetString("CommandField_CallbacksNotSupported", new object[] { base.Control.ID }));
            }
        }

        [UrlProperty, WebCategory("Appearance"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("CommandField_CancelImageUrl")]
        public virtual string CancelImageUrl
        {
            get
            {
                object obj2 = base.ViewState["CancelImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["CancelImageUrl"]))
                {
                    base.ViewState["CancelImageUrl"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebSysDefaultValue("CommandField_DefaultCancelCaption"), Localizable(true), WebCategory("Appearance"), WebSysDescription("CommandField_CancelText")]
        public virtual string CancelText
        {
            get
            {
                object obj2 = base.ViewState["CancelText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CommandField_DefaultCancelCaption");
            }
            set
            {
                if (!object.Equals(value, base.ViewState["CancelText"]))
                {
                    base.ViewState["CancelText"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebSysDescription("ButtonFieldBase_CausesValidation"), WebCategory("Behavior"), DefaultValue(true)]
        public override bool CausesValidation
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
                base.CausesValidation = value;
            }
        }

        [DefaultValue(""), UrlProperty, WebCategory("Appearance"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("CommandField_DeleteImageUrl")]
        public virtual string DeleteImageUrl
        {
            get
            {
                object obj2 = base.ViewState["DeleteImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["DeleteImageUrl"]))
                {
                    base.ViewState["DeleteImageUrl"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Appearance"), WebSysDefaultValue("CommandField_DefaultDeleteCaption"), WebSysDescription("CommandField_DeleteText"), Localizable(true)]
        public virtual string DeleteText
        {
            get
            {
                object obj2 = base.ViewState["DeleteText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CommandField_DefaultDeleteCaption");
            }
            set
            {
                if (!object.Equals(value, base.ViewState["DeleteText"]))
                {
                    base.ViewState["DeleteText"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Appearance"), DefaultValue(""), UrlProperty, WebSysDescription("CommandField_EditImageUrl")]
        public virtual string EditImageUrl
        {
            get
            {
                object obj2 = base.ViewState["EditImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["EditImageUrl"]))
                {
                    base.ViewState["EditImageUrl"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Appearance"), WebSysDescription("CommandField_EditText"), Localizable(true), WebSysDefaultValue("CommandField_DefaultEditCaption")]
        public virtual string EditText
        {
            get
            {
                object obj2 = base.ViewState["EditText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CommandField_DefaultEditCaption");
            }
            set
            {
                if (!object.Equals(value, base.ViewState["EditText"]))
                {
                    base.ViewState["EditText"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Appearance"), UrlProperty, DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("CommandField_InsertImageUrl")]
        public virtual string InsertImageUrl
        {
            get
            {
                object obj2 = base.ViewState["InsertImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["InsertImageUrl"]))
                {
                    base.ViewState["InsertImageUrl"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [Localizable(true), WebCategory("Appearance"), WebSysDescription("CommandField_InsertText"), WebSysDefaultValue("CommandField_DefaultInsertCaption")]
        public virtual string InsertText
        {
            get
            {
                object obj2 = base.ViewState["InsertText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CommandField_DefaultInsertCaption");
            }
            set
            {
                if (!object.Equals(value, base.ViewState["InsertText"]))
                {
                    base.ViewState["InsertText"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [UrlProperty, WebCategory("Appearance"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("CommandField_NewImageUrl")]
        public virtual string NewImageUrl
        {
            get
            {
                object obj2 = base.ViewState["NewImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["NewImageUrl"]))
                {
                    base.ViewState["NewImageUrl"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Appearance"), WebSysDescription("CommandField_NewText"), Localizable(true), WebSysDefaultValue("CommandField_DefaultNewCaption")]
        public virtual string NewText
        {
            get
            {
                object obj2 = base.ViewState["NewText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CommandField_DefaultNewCaption");
            }
            set
            {
                if (!object.Equals(value, base.ViewState["NewText"]))
                {
                    base.ViewState["NewText"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Appearance"), UrlProperty, DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("CommandField_SelectImageUrl")]
        public virtual string SelectImageUrl
        {
            get
            {
                object obj2 = base.ViewState["SelectImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["SelectImageUrl"]))
                {
                    base.ViewState["SelectImageUrl"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebSysDescription("CommandField_SelectText"), Localizable(true), WebCategory("Appearance"), WebSysDefaultValue("CommandField_DefaultSelectCaption")]
        public virtual string SelectText
        {
            get
            {
                object obj2 = base.ViewState["SelectText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CommandField_DefaultSelectCaption");
            }
            set
            {
                if (!object.Equals(value, base.ViewState["SelectText"]))
                {
                    base.ViewState["SelectText"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebSysDescription("CommandField_ShowCancelButton"), WebCategory("Behavior"), DefaultValue(true)]
        public virtual bool ShowCancelButton
        {
            get
            {
                object obj2 = base.ViewState["ShowCancelButton"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                object obj2 = base.ViewState["ShowCancelButton"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    base.ViewState["ShowCancelButton"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [DefaultValue(false), WebSysDescription("CommandField_ShowDeleteButton"), WebCategory("Behavior")]
        public virtual bool ShowDeleteButton
        {
            get
            {
                object obj2 = base.ViewState["ShowDeleteButton"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                object obj2 = base.ViewState["ShowDeleteButton"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    base.ViewState["ShowDeleteButton"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [DefaultValue(false), WebCategory("Behavior"), WebSysDescription("CommandField_ShowEditButton")]
        public virtual bool ShowEditButton
        {
            get
            {
                object obj2 = base.ViewState["ShowEditButton"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                object obj2 = base.ViewState["ShowEditButton"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    base.ViewState["ShowEditButton"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [DefaultValue(false), WebCategory("Behavior"), WebSysDescription("CommandField_ShowInsertButton")]
        public virtual bool ShowInsertButton
        {
            get
            {
                object obj2 = base.ViewState["ShowInsertButton"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                object obj2 = base.ViewState["ShowInsertButton"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    base.ViewState["ShowInsertButton"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Behavior"), WebSysDescription("CommandField_ShowSelectButton"), DefaultValue(false)]
        public virtual bool ShowSelectButton
        {
            get
            {
                object obj2 = base.ViewState["ShowSelectButton"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                object obj2 = base.ViewState["ShowSelectButton"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    base.ViewState["ShowSelectButton"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Appearance"), UrlProperty, DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("CommandField_UpdateImageUrl")]
        public virtual string UpdateImageUrl
        {
            get
            {
                object obj2 = base.ViewState["UpdateImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["UpdateImageUrl"]))
                {
                    base.ViewState["UpdateImageUrl"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Appearance"), WebSysDescription("CommandField_UpdateText"), Localizable(true), WebSysDefaultValue("CommandField_DefaultUpdateCaption")]
        public virtual string UpdateText
        {
            get
            {
                object obj2 = base.ViewState["UpdateText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("CommandField_DefaultUpdateCaption");
            }
            set
            {
                if (!object.Equals(value, base.ViewState["UpdateText"]))
                {
                    base.ViewState["UpdateText"] = value;
                    this.OnFieldChanged();
                }
            }
        }
    }
}

