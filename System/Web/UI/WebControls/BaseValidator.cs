namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [Designer("System.Web.UI.Design.WebControls.BaseValidatorDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("ErrorMessage")]
    public abstract class BaseValidator : Label, IValidator
    {
        private bool isValid = true;
        private bool preRenderCalled;
        private bool propertiesChecked = false;
        private bool propertiesValid = true;
        private bool renderUplevel = false;
        private const string ValidatorFileName = "WebUIValidation.js";
        private const string ValidatorIncludeScriptKey = "ValidatorIncludeScript";
        private const string ValidatorStartupScript = "\r\nvar Page_ValidationActive = false;\r\nif (typeof(ValidatorOnLoad) == \"function\") {\r\n    ValidatorOnLoad();\r\n}\r\n\r\nfunction ValidatorOnSubmit() {\r\n    if (Page_ValidationActive) {\r\n        return ValidatorCommonOnSubmit();\r\n    }\r\n    else {\r\n        return true;\r\n    }\r\n}\r\n        ";
        private bool wasForeColorSet;

        protected BaseValidator()
        {
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            bool flag = !this.Enabled;
            if (flag)
            {
                this.Enabled = true;
            }
            try
            {
                if (this.RenderUplevel)
                {
                    base.EnsureID();
                    string clientID = this.ClientID;
                    HtmlTextWriter writer2 = base.EnableLegacyRendering ? writer : null;
                    if (this.ControlToValidate.Length > 0)
                    {
                        this.AddExpandoAttribute(writer2, clientID, "controltovalidate", this.GetControlRenderID(this.ControlToValidate));
                    }
                    if (this.SetFocusOnError)
                    {
                        this.AddExpandoAttribute(writer2, clientID, "focusOnError", "t", false);
                    }
                    if (this.ErrorMessage.Length > 0)
                    {
                        this.AddExpandoAttribute(writer2, clientID, "errormessage", this.ErrorMessage);
                    }
                    ValidatorDisplay enumValue = this.Display;
                    if (enumValue != ValidatorDisplay.Static)
                    {
                        this.AddExpandoAttribute(writer2, clientID, "display", PropertyConverter.EnumToString(typeof(ValidatorDisplay), enumValue), false);
                    }
                    if (!this.IsValid)
                    {
                        this.AddExpandoAttribute(writer2, clientID, "isvalid", "False", false);
                    }
                    if (flag)
                    {
                        this.AddExpandoAttribute(writer2, clientID, "enabled", "False", false);
                    }
                    if (this.ValidationGroup.Length > 0)
                    {
                        this.AddExpandoAttribute(writer2, clientID, "validationGroup", this.ValidationGroup);
                    }
                }
                base.AddAttributesToRender(writer);
            }
            finally
            {
                if (flag)
                {
                    this.Enabled = false;
                }
            }
        }

        internal void AddExpandoAttribute(HtmlTextWriter writer, string controlId, string attributeName, string attributeValue)
        {
            this.AddExpandoAttribute(writer, controlId, attributeName, attributeValue, true);
        }

        internal void AddExpandoAttribute(HtmlTextWriter writer, string controlId, string attributeName, string attributeValue, bool encode)
        {
            AddExpandoAttribute(this, writer, controlId, attributeName, attributeValue, encode);
        }

        internal static void AddExpandoAttribute(Control control, HtmlTextWriter writer, string controlId, string attributeName, string attributeValue, bool encode)
        {
            if (writer != null)
            {
                writer.AddAttribute(attributeName, attributeValue, encode);
            }
            else
            {
                Page page = control.Page;
                if (!page.IsPartialRenderingSupported)
                {
                    page.ClientScript.RegisterExpandoAttribute(controlId, attributeName, attributeValue, encode);
                }
                else
                {
                    ValidatorCompatibilityHelper.RegisterExpandoAttribute(control, controlId, attributeName, attributeValue, encode);
                }
            }
        }

        protected void CheckControlValidationProperty(string name, string propertyName)
        {
            Control component = this.NamingContainer.FindControl(name);
            if (component == null)
            {
                throw new HttpException(System.Web.SR.GetString("Validator_control_not_found", new object[] { name, propertyName, this.ID }));
            }
            if (GetValidationProperty(component) == null)
            {
                throw new HttpException(System.Web.SR.GetString("Validator_bad_control_type", new object[] { name, propertyName, this.ID }));
            }
        }

        protected virtual bool ControlPropertiesValid()
        {
            string controlToValidate = this.ControlToValidate;
            if (controlToValidate.Length == 0)
            {
                throw new HttpException(System.Web.SR.GetString("Validator_control_blank", new object[] { this.ID }));
            }
            this.CheckControlValidationProperty(controlToValidate, "ControlToValidate");
            return true;
        }

        protected virtual bool DetermineRenderUplevel()
        {
            Page page = this.Page;
            if ((page == null) || (page.RequestInternal == null))
            {
                return false;
            }
            return ((this.EnableClientScript && (page.Request.Browser.W3CDomVersion.Major >= 1)) && (page.Request.Browser.EcmaScriptVersion.CompareTo(new Version(1, 2)) >= 0));
        }

        protected abstract bool EvaluateIsValid();
        protected string GetControlRenderID(string name)
        {
            Control control = this.FindControl(name);
            if (control == null)
            {
                return string.Empty;
            }
            return control.ClientID;
        }

        protected string GetControlValidationValue(string name)
        {
            Control component = this.NamingContainer.FindControl(name);
            if (component == null)
            {
                return null;
            }
            PropertyDescriptor validationProperty = GetValidationProperty(component);
            if (validationProperty == null)
            {
                return null;
            }
            object obj2 = validationProperty.GetValue(component);
            if (obj2 is ListItem)
            {
                return ((ListItem) obj2).Value;
            }
            if (obj2 != null)
            {
                return obj2.ToString();
            }
            return string.Empty;
        }

        public static PropertyDescriptor GetValidationProperty(object component)
        {
            ValidationPropertyAttribute attribute = (ValidationPropertyAttribute) TypeDescriptor.GetAttributes(component)[typeof(ValidationPropertyAttribute)];
            if ((attribute != null) && (attribute.Name != null))
            {
                return TypeDescriptor.GetProperties(component, (Attribute[]) null)[attribute.Name];
            }
            return null;
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!this.wasForeColorSet && (this.RenderingCompatibility < VersionUtil.Framework40))
            {
                this.ForeColor = Color.Red;
            }
            this.Page.Validators.Add(this);
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.preRenderCalled = true;
            this.propertiesChecked = false;
            bool propertiesValid = this.PropertiesValid;
            this.renderUplevel = this.DetermineRenderUplevel();
            if (this.renderUplevel)
            {
                this.RegisterValidatorCommonScript();
            }
        }

        protected internal override void OnUnload(EventArgs e)
        {
            if (this.Page != null)
            {
                this.Page.Validators.Remove(this);
            }
            base.OnUnload(e);
        }

        protected void RegisterValidatorCommonScript()
        {
            if (!this.Page.IsPartialRenderingSupported)
            {
                if (!this.Page.ClientScript.IsClientScriptBlockRegistered(typeof(BaseValidator), "ValidatorIncludeScript"))
                {
                    this.Page.ClientScript.RegisterClientScriptResource(typeof(BaseValidator), "WebUIValidation.js");
                    this.Page.ClientScript.RegisterStartupScript(typeof(BaseValidator), "ValidatorIncludeScript", "\r\nvar Page_ValidationActive = false;\r\nif (typeof(ValidatorOnLoad) == \"function\") {\r\n    ValidatorOnLoad();\r\n}\r\n\r\nfunction ValidatorOnSubmit() {\r\n    if (Page_ValidationActive) {\r\n        return ValidatorCommonOnSubmit();\r\n    }\r\n    else {\r\n        return true;\r\n    }\r\n}\r\n        ", true);
                    this.Page.ClientScript.RegisterOnSubmitStatement(typeof(BaseValidator), "ValidatorOnSubmit", "if (typeof(ValidatorOnSubmit) == \"function\" && ValidatorOnSubmit() == false) return false;");
                }
            }
            else
            {
                ValidatorCompatibilityHelper.RegisterClientScriptResource(this, typeof(BaseValidator), "WebUIValidation.js");
                ValidatorCompatibilityHelper.RegisterStartupScript(this, typeof(BaseValidator), "ValidatorIncludeScript", "\r\nvar Page_ValidationActive = false;\r\nif (typeof(ValidatorOnLoad) == \"function\") {\r\n    ValidatorOnLoad();\r\n}\r\n\r\nfunction ValidatorOnSubmit() {\r\n    if (Page_ValidationActive) {\r\n        return ValidatorCommonOnSubmit();\r\n    }\r\n    else {\r\n        return true;\r\n    }\r\n}\r\n        ", true);
                ValidatorCompatibilityHelper.RegisterOnSubmitStatement(this, typeof(BaseValidator), "ValidatorOnSubmit", "if (typeof(ValidatorOnSubmit) == \"function\" && ValidatorOnSubmit() == false) return false;");
            }
        }

        protected virtual void RegisterValidatorDeclaration()
        {
            string arrayValue = "document.getElementById(\"" + this.ClientID + "\")";
            if (!this.Page.IsPartialRenderingSupported)
            {
                this.Page.ClientScript.RegisterArrayDeclaration("Page_Validators", arrayValue);
            }
            else
            {
                ValidatorCompatibilityHelper.RegisterArrayDeclaration(this, "Page_Validators", arrayValue);
                ValidatorCompatibilityHelper.RegisterStartupScript(this, typeof(BaseValidator), this.ClientID + "_DisposeScript", string.Format(CultureInfo.InvariantCulture, "\r\ndocument.getElementById('{0}').dispose = function() {{\r\n    Array.remove({1}, document.getElementById('{0}'));\r\n}}\r\n", new object[] { this.ClientID, "Page_Validators" }), true);
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            bool flag;
            if (base.DesignMode || (!this.preRenderCalled && (this.Page == null)))
            {
                this.propertiesChecked = true;
                this.propertiesValid = true;
                this.renderUplevel = false;
                flag = true;
            }
            else
            {
                flag = this.Enabled && !this.IsValid;
            }
            if (this.PropertiesValid)
            {
                bool flag2;
                bool flag3;
                if (this.Page != null)
                {
                    this.Page.VerifyRenderingInServerForm(this);
                }
                ValidatorDisplay display = this.Display;
                if (this.RenderUplevel)
                {
                    flag3 = true;
                    flag2 = display != ValidatorDisplay.None;
                }
                else
                {
                    flag2 = (display != ValidatorDisplay.None) && flag;
                    flag3 = flag2;
                }
                if (flag3 && this.RenderUplevel)
                {
                    this.RegisterValidatorDeclaration();
                    if ((display == ValidatorDisplay.None) || (!flag && (display == ValidatorDisplay.Dynamic)))
                    {
                        base.Style["display"] = "none";
                    }
                    else if (!flag)
                    {
                        base.Style["visibility"] = "hidden";
                    }
                }
                if (flag3)
                {
                    this.RenderBeginTag(writer);
                }
                if (flag2)
                {
                    if (this.Text.Trim().Length > 0)
                    {
                        this.RenderContents(writer);
                    }
                    else if (base.HasRenderingData())
                    {
                        base.RenderContents(writer);
                    }
                    else
                    {
                        writer.Write(this.ErrorMessage);
                    }
                }
                else if (!this.RenderUplevel && (display == ValidatorDisplay.Static))
                {
                    writer.Write("&nbsp;");
                }
                if (flag3)
                {
                    this.RenderEndTag(writer);
                }
            }
        }

        internal bool ShouldSerializeForeColor()
        {
            Color color = (this.RenderingCompatibility < VersionUtil.Framework40) ? Color.Red : Color.Empty;
            return (color != this.ForeColor);
        }

        public void Validate()
        {
            this.IsValid = true;
            if (this.Visible && this.Enabled)
            {
                this.propertiesChecked = false;
                if (this.PropertiesValid)
                {
                    this.IsValid = this.EvaluateIsValid();
                    if ((!this.IsValid && (this.Page != null)) && this.SetFocusOnError)
                    {
                        string controlToValidate = this.ControlToValidate;
                        Control control = this.NamingContainer.FindControl(controlToValidate);
                        if (control != null)
                        {
                            controlToValidate = control.ClientID;
                        }
                        this.Page.SetValidatorInvalidControlFocus(controlToValidate);
                    }
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string AssociatedControlID
        {
            get
            {
                return base.AssociatedControlID;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("Property_Not_Supported", new object[] { "AssociatedControlID", base.GetType().ToString() }));
            }
        }

        [TypeConverter(typeof(ValidatedControlConverter)), WebCategory("Behavior"), Themeable(false), DefaultValue(""), IDReferenceProperty, WebSysDescription("BaseValidator_ControlToValidate")]
        public string ControlToValidate
        {
            get
            {
                object obj2 = this.ViewState["ControlToValidate"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["ControlToValidate"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("BaseValidator_Display"), DefaultValue(1), Themeable(true)]
        public ValidatorDisplay Display
        {
            get
            {
                object obj2 = this.ViewState["Display"];
                if (obj2 != null)
                {
                    return (ValidatorDisplay) obj2;
                }
                return ValidatorDisplay.Static;
            }
            set
            {
                if ((value < ValidatorDisplay.None) || (value > ValidatorDisplay.Dynamic))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["Display"] = value;
            }
        }

        [WebSysDescription("BaseValidator_EnableClientScript"), WebCategory("Behavior"), Themeable(false), DefaultValue(true)]
        public bool EnableClientScript
        {
            get
            {
                object obj2 = this.ViewState["EnableClientScript"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["EnableClientScript"] = value;
            }
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
                if (!value)
                {
                    this.isValid = true;
                }
            }
        }

        [DefaultValue(""), WebSysDescription("BaseValidator_ErrorMessage"), Localizable(true), WebCategory("Appearance")]
        public string ErrorMessage
        {
            get
            {
                object obj2 = this.ViewState["ErrorMessage"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["ErrorMessage"] = value;
            }
        }

        [DefaultValue(typeof(Color), "Red")]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                this.wasForeColorSet = true;
                base.ForeColor = value;
            }
        }

        internal override bool IsReloadable
        {
            get
            {
                return true;
            }
        }

        [Themeable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), WebCategory("Behavior"), DefaultValue(true), WebSysDescription("BaseValidator_IsValid")]
        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
            set
            {
                this.isValid = value;
            }
        }

        protected bool PropertiesValid
        {
            get
            {
                if (!this.propertiesChecked)
                {
                    this.propertiesValid = this.ControlPropertiesValid();
                    this.propertiesChecked = true;
                }
                return this.propertiesValid;
            }
        }

        protected bool RenderUplevel
        {
            get
            {
                return this.renderUplevel;
            }
        }

        [WebSysDescription("BaseValidator_SetFocusOnError"), DefaultValue(false), WebCategory("Behavior"), Themeable(false)]
        public bool SetFocusOnError
        {
            get
            {
                object obj2 = this.ViewState["SetFocusOnError"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["SetFocusOnError"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("BaseValidator_Text"), PersistenceMode(PersistenceMode.InnerDefaultProperty), DefaultValue("")]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        [DefaultValue(""), WebSysDescription("BaseValidator_ValidationGroup"), Themeable(false), WebCategory("Behavior")]
        public virtual string ValidationGroup
        {
            get
            {
                object obj2 = this.ViewState["ValidationGroup"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["ValidationGroup"] = value;
            }
        }
    }
}

