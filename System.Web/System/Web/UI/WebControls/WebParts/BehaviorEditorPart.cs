namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public sealed class BehaviorEditorPart : EditorPart
    {
        private CheckBox _allowClose;
        private string _allowCloseErrorMessage;
        private CheckBox _allowConnect;
        private string _allowConnectErrorMessage;
        private CheckBox _allowEdit;
        private string _allowEditErrorMessage;
        private CheckBox _allowHide;
        private string _allowHideErrorMessage;
        private CheckBox _allowMinimize;
        private string _allowMinimizeErrorMessage;
        private CheckBox _allowZoneChange;
        private string _allowZoneChangeErrorMessage;
        private TextBox _authorizationFilter;
        private string _authorizationFilterErrorMessage;
        private TextBox _catalogIconImageUrl;
        private string _catalogIconImageUrlErrorMessage;
        private TextBox _description;
        private string _descriptionErrorMessage;
        private DropDownList _exportMode;
        private string _exportModeErrorMessage;
        private DropDownList _helpMode;
        private string _helpModeErrorMessage;
        private TextBox _helpUrl;
        private string _helpUrlErrorMessage;
        private TextBox _importErrorMessage;
        private string _importErrorMessageErrorMessage;
        private TextBox _titleIconImageUrl;
        private string _titleIconImageUrlErrorMessage;
        private TextBox _titleUrl;
        private string _titleUrlErrorMessage;
        private const int TextBoxColumns = 30;

        public override bool ApplyChanges()
        {
            WebPart webPartToEdit = base.WebPartToEdit;
            if (webPartToEdit != null)
            {
                this.EnsureChildControls();
                bool allowLayoutChange = webPartToEdit.Zone.AllowLayoutChange;
                if (allowLayoutChange)
                {
                    try
                    {
                        webPartToEdit.AllowClose = this._allowClose.Checked;
                    }
                    catch (Exception exception)
                    {
                        this._allowCloseErrorMessage = base.CreateErrorMessage(exception.Message);
                    }
                }
                try
                {
                    webPartToEdit.AllowConnect = this._allowConnect.Checked;
                }
                catch (Exception exception2)
                {
                    this._allowConnectErrorMessage = base.CreateErrorMessage(exception2.Message);
                }
                if (allowLayoutChange)
                {
                    try
                    {
                        webPartToEdit.AllowHide = this._allowHide.Checked;
                    }
                    catch (Exception exception3)
                    {
                        this._allowHideErrorMessage = base.CreateErrorMessage(exception3.Message);
                    }
                }
                if (allowLayoutChange)
                {
                    try
                    {
                        webPartToEdit.AllowMinimize = this._allowMinimize.Checked;
                    }
                    catch (Exception exception4)
                    {
                        this._allowMinimizeErrorMessage = base.CreateErrorMessage(exception4.Message);
                    }
                }
                if (allowLayoutChange)
                {
                    try
                    {
                        webPartToEdit.AllowZoneChange = this._allowZoneChange.Checked;
                    }
                    catch (Exception exception5)
                    {
                        this._allowZoneChangeErrorMessage = base.CreateErrorMessage(exception5.Message);
                    }
                }
                try
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(WebPartExportMode));
                    webPartToEdit.ExportMode = (WebPartExportMode) converter.ConvertFromString(this._exportMode.SelectedValue);
                }
                catch (Exception exception6)
                {
                    this._exportModeErrorMessage = base.CreateErrorMessage(exception6.Message);
                }
                try
                {
                    TypeConverter converter2 = TypeDescriptor.GetConverter(typeof(WebPartHelpMode));
                    webPartToEdit.HelpMode = (WebPartHelpMode) converter2.ConvertFromString(this._helpMode.SelectedValue);
                }
                catch (Exception exception7)
                {
                    this._helpModeErrorMessage = base.CreateErrorMessage(exception7.Message);
                }
                try
                {
                    webPartToEdit.Description = this._description.Text;
                }
                catch (Exception exception8)
                {
                    this._descriptionErrorMessage = base.CreateErrorMessage(exception8.Message);
                }
                string text = this._titleUrl.Text;
                if (CrossSiteScriptingValidation.IsDangerousUrl(text))
                {
                    this._titleUrlErrorMessage = System.Web.SR.GetString("EditorPart_ErrorBadUrl");
                }
                else
                {
                    try
                    {
                        webPartToEdit.TitleUrl = text;
                    }
                    catch (Exception exception9)
                    {
                        this._titleUrlErrorMessage = base.CreateErrorMessage(exception9.Message);
                    }
                }
                text = this._titleIconImageUrl.Text;
                if (CrossSiteScriptingValidation.IsDangerousUrl(text))
                {
                    this._titleIconImageUrlErrorMessage = System.Web.SR.GetString("EditorPart_ErrorBadUrl");
                }
                else
                {
                    try
                    {
                        webPartToEdit.TitleIconImageUrl = text;
                    }
                    catch (Exception exception10)
                    {
                        this._titleIconImageUrlErrorMessage = base.CreateErrorMessage(exception10.Message);
                    }
                }
                text = this._catalogIconImageUrl.Text;
                if (CrossSiteScriptingValidation.IsDangerousUrl(text))
                {
                    this._catalogIconImageUrlErrorMessage = System.Web.SR.GetString("EditorPart_ErrorBadUrl");
                }
                else
                {
                    try
                    {
                        webPartToEdit.CatalogIconImageUrl = text;
                    }
                    catch (Exception exception11)
                    {
                        this._catalogIconImageUrlErrorMessage = base.CreateErrorMessage(exception11.Message);
                    }
                }
                text = this._helpUrl.Text;
                if (CrossSiteScriptingValidation.IsDangerousUrl(text))
                {
                    this._helpUrlErrorMessage = System.Web.SR.GetString("EditorPart_ErrorBadUrl");
                }
                else
                {
                    try
                    {
                        webPartToEdit.HelpUrl = text;
                    }
                    catch (Exception exception12)
                    {
                        this._helpUrlErrorMessage = base.CreateErrorMessage(exception12.Message);
                    }
                }
                try
                {
                    webPartToEdit.ImportErrorMessage = this._importErrorMessage.Text;
                }
                catch (Exception exception13)
                {
                    this._importErrorMessageErrorMessage = base.CreateErrorMessage(exception13.Message);
                }
                try
                {
                    webPartToEdit.AuthorizationFilter = this._authorizationFilter.Text;
                }
                catch (Exception exception14)
                {
                    this._authorizationFilterErrorMessage = base.CreateErrorMessage(exception14.Message);
                }
                try
                {
                    webPartToEdit.AllowEdit = this._allowEdit.Checked;
                }
                catch (Exception exception15)
                {
                    this._allowEditErrorMessage = base.CreateErrorMessage(exception15.Message);
                }
            }
            return !this.HasError;
        }

        protected internal override void CreateChildControls()
        {
            ControlCollection controls = this.Controls;
            controls.Clear();
            this._allowClose = new CheckBox();
            controls.Add(this._allowClose);
            this._allowConnect = new CheckBox();
            controls.Add(this._allowConnect);
            this._allowHide = new CheckBox();
            controls.Add(this._allowHide);
            this._allowMinimize = new CheckBox();
            controls.Add(this._allowMinimize);
            this._allowZoneChange = new CheckBox();
            controls.Add(this._allowZoneChange);
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(WebPartExportMode));
            this._exportMode = new DropDownList();
            this._exportMode.Items.AddRange(new ListItem[] { new ListItem(System.Web.SR.GetString("BehaviorEditorPart_ExportModeNone"), converter.ConvertToString(WebPartExportMode.None)), new ListItem(System.Web.SR.GetString("BehaviorEditorPart_ExportModeAll"), converter.ConvertToString(WebPartExportMode.All)), new ListItem(System.Web.SR.GetString("BehaviorEditorPart_ExportModeNonSensitiveData"), converter.ConvertToString(WebPartExportMode.NonSensitiveData)) });
            controls.Add(this._exportMode);
            TypeConverter converter2 = TypeDescriptor.GetConverter(typeof(WebPartHelpMode));
            this._helpMode = new DropDownList();
            this._helpMode.Items.AddRange(new ListItem[] { new ListItem(System.Web.SR.GetString("BehaviorEditorPart_HelpModeModal"), converter2.ConvertToString(WebPartHelpMode.Modal)), new ListItem(System.Web.SR.GetString("BehaviorEditorPart_HelpModeModeless"), converter2.ConvertToString(WebPartHelpMode.Modeless)), new ListItem(System.Web.SR.GetString("BehaviorEditorPart_HelpModeNavigate"), converter2.ConvertToString(WebPartHelpMode.Navigate)) });
            controls.Add(this._helpMode);
            this._description = new TextBox();
            this._description.Columns = 30;
            controls.Add(this._description);
            this._titleUrl = new TextBox();
            this._titleUrl.Columns = 30;
            controls.Add(this._titleUrl);
            this._titleIconImageUrl = new TextBox();
            this._titleIconImageUrl.Columns = 30;
            controls.Add(this._titleIconImageUrl);
            this._catalogIconImageUrl = new TextBox();
            this._catalogIconImageUrl.Columns = 30;
            controls.Add(this._catalogIconImageUrl);
            this._helpUrl = new TextBox();
            this._helpUrl.Columns = 30;
            controls.Add(this._helpUrl);
            this._importErrorMessage = new TextBox();
            this._importErrorMessage.Columns = 30;
            controls.Add(this._importErrorMessage);
            this._authorizationFilter = new TextBox();
            this._authorizationFilter.Columns = 30;
            controls.Add(this._authorizationFilter);
            this._allowEdit = new CheckBox();
            controls.Add(this._allowEdit);
            foreach (Control control in controls)
            {
                control.EnableViewState = false;
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if ((this.Display && this.Visible) && !this.HasError)
            {
                this.SyncChanges();
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            this.EnsureChildControls();
            string[] propertyDisplayNames = new string[] { System.Web.SR.GetString("BehaviorEditorPart_Description"), System.Web.SR.GetString("BehaviorEditorPart_TitleLink"), System.Web.SR.GetString("BehaviorEditorPart_TitleIconImageLink"), System.Web.SR.GetString("BehaviorEditorPart_CatalogIconImageLink"), System.Web.SR.GetString("BehaviorEditorPart_HelpLink"), System.Web.SR.GetString("BehaviorEditorPart_HelpMode"), System.Web.SR.GetString("BehaviorEditorPart_ImportErrorMessage"), System.Web.SR.GetString("BehaviorEditorPart_ExportMode"), System.Web.SR.GetString("BehaviorEditorPart_AuthorizationFilter"), System.Web.SR.GetString("BehaviorEditorPart_AllowClose"), System.Web.SR.GetString("BehaviorEditorPart_AllowConnect"), System.Web.SR.GetString("BehaviorEditorPart_AllowEdit"), System.Web.SR.GetString("BehaviorEditorPart_AllowHide"), System.Web.SR.GetString("BehaviorEditorPart_AllowMinimize"), System.Web.SR.GetString("BehaviorEditorPart_AllowZoneChange") };
            WebControl[] propertyEditors = new WebControl[] { this._description, this._titleUrl, this._titleIconImageUrl, this._catalogIconImageUrl, this._helpUrl, this._helpMode, this._importErrorMessage, this._exportMode, this._authorizationFilter, this._allowClose, this._allowConnect, this._allowEdit, this._allowHide, this._allowMinimize, this._allowZoneChange };
            string[] errorMessages = new string[] { this._descriptionErrorMessage, this._titleUrlErrorMessage, this._titleIconImageUrlErrorMessage, this._catalogIconImageUrlErrorMessage, this._helpUrlErrorMessage, this._helpModeErrorMessage, this._importErrorMessageErrorMessage, this._exportModeErrorMessage, this._authorizationFilterErrorMessage, this._allowCloseErrorMessage, this._allowConnectErrorMessage, this._allowEditErrorMessage, this._allowHideErrorMessage, this._allowMinimizeErrorMessage, this._allowZoneChangeErrorMessage };
            base.RenderPropertyEditors(writer, propertyDisplayNames, null, propertyEditors, errorMessages);
        }

        public override void SyncChanges()
        {
            WebPart webPartToEdit = base.WebPartToEdit;
            if (webPartToEdit != null)
            {
                bool allowLayoutChange = webPartToEdit.Zone.AllowLayoutChange;
                this.EnsureChildControls();
                this._allowClose.Checked = webPartToEdit.AllowClose;
                this._allowClose.Enabled = allowLayoutChange;
                this._allowConnect.Checked = webPartToEdit.AllowConnect;
                this._allowHide.Checked = webPartToEdit.AllowHide;
                this._allowHide.Enabled = allowLayoutChange;
                this._allowMinimize.Checked = webPartToEdit.AllowMinimize;
                this._allowMinimize.Enabled = allowLayoutChange;
                this._allowZoneChange.Checked = webPartToEdit.AllowZoneChange;
                this._allowZoneChange.Enabled = allowLayoutChange;
                this._exportMode.SelectedValue = TypeDescriptor.GetConverter(typeof(WebPartExportMode)).ConvertToString(webPartToEdit.ExportMode);
                this._helpMode.SelectedValue = TypeDescriptor.GetConverter(typeof(WebPartHelpMode)).ConvertToString(webPartToEdit.HelpMode);
                this._description.Text = webPartToEdit.Description;
                this._titleUrl.Text = webPartToEdit.TitleUrl;
                this._titleIconImageUrl.Text = webPartToEdit.TitleIconImageUrl;
                this._catalogIconImageUrl.Text = webPartToEdit.CatalogIconImageUrl;
                this._helpUrl.Text = webPartToEdit.HelpUrl;
                this._importErrorMessage.Text = webPartToEdit.ImportErrorMessage;
                this._authorizationFilter.Text = webPartToEdit.AuthorizationFilter;
                this._allowEdit.Checked = webPartToEdit.AllowEdit;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override string DefaultButton
        {
            get
            {
                return base.DefaultButton;
            }
            set
            {
                base.DefaultButton = value;
            }
        }

        public override bool Display
        {
            get
            {
                if (((base.WebPartToEdit != null) && base.WebPartToEdit.IsShared) && ((base.WebPartManager != null) && (base.WebPartManager.Personalization.Scope == PersonalizationScope.User)))
                {
                    return false;
                }
                return base.Display;
            }
        }

        private bool HasError
        {
            get
            {
                if (((((this._allowCloseErrorMessage == null) && (this._allowConnectErrorMessage == null)) && ((this._allowHideErrorMessage == null) && (this._allowMinimizeErrorMessage == null))) && (((this._allowZoneChangeErrorMessage == null) && (this._exportModeErrorMessage == null)) && ((this._helpModeErrorMessage == null) && (this._descriptionErrorMessage == null)))) && ((((this._titleUrlErrorMessage == null) && (this._titleIconImageUrlErrorMessage == null)) && ((this._catalogIconImageUrlErrorMessage == null) && (this._helpUrlErrorMessage == null))) && ((this._importErrorMessageErrorMessage == null) && (this._authorizationFilterErrorMessage == null))))
                {
                    return (this._allowEditErrorMessage != null);
                }
                return true;
            }
        }

        [WebSysDefaultValue("BehaviorEditorPart_PartTitle")]
        public override string Title
        {
            get
            {
                string str = (string) this.ViewState["Title"];
                if (str == null)
                {
                    return System.Web.SR.GetString("BehaviorEditorPart_PartTitle");
                }
                return str;
            }
            set
            {
                this.ViewState["Title"] = value;
            }
        }
    }
}

