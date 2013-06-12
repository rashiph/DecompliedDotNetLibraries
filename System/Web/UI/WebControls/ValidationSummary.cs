namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [Designer("System.Web.UI.Design.WebControls.ValidationSummaryDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class ValidationSummary : WebControl
    {
        private const string breakTag = "b";
        private bool renderUplevel;
        private bool wasForeColorSet;

        public ValidationSummary() : base(HtmlTextWriterTag.Div)
        {
            this.renderUplevel = false;
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            if (this.renderUplevel)
            {
                base.EnsureID();
                string clientID = this.ClientID;
                HtmlTextWriter writer2 = base.EnableLegacyRendering ? writer : null;
                if (this.HeaderText.Length > 0)
                {
                    BaseValidator.AddExpandoAttribute(this, writer2, clientID, "headertext", this.HeaderText, true);
                }
                if (this.ShowMessageBox)
                {
                    BaseValidator.AddExpandoAttribute(this, writer2, clientID, "showmessagebox", "True", false);
                }
                if (!this.ShowSummary)
                {
                    BaseValidator.AddExpandoAttribute(this, writer2, clientID, "showsummary", "False", false);
                }
                if (this.DisplayMode != ValidationSummaryDisplayMode.BulletList)
                {
                    BaseValidator.AddExpandoAttribute(this, writer2, clientID, "displaymode", PropertyConverter.EnumToString(typeof(ValidationSummaryDisplayMode), this.DisplayMode), false);
                }
                if (this.ValidationGroup.Length > 0)
                {
                    BaseValidator.AddExpandoAttribute(this, writer2, clientID, "validationGroup", this.ValidationGroup, true);
                }
            }
            base.AddAttributesToRender(writer);
        }

        internal string[] GetErrorMessages(out bool inError)
        {
            string[] strArray = null;
            inError = false;
            int num = 0;
            ValidatorCollection validators = this.Page.GetValidators(this.ValidationGroup);
            for (int i = 0; i < validators.Count; i++)
            {
                IValidator validator = validators[i];
                if (!validator.IsValid)
                {
                    inError = true;
                    if (validator.ErrorMessage.Length != 0)
                    {
                        num++;
                    }
                }
            }
            if (num != 0)
            {
                strArray = new string[num];
                int index = 0;
                for (int j = 0; j < validators.Count; j++)
                {
                    IValidator validator2 = validators[j];
                    if ((!validator2.IsValid && (validator2.ErrorMessage != null)) && (validator2.ErrorMessage.Length != 0))
                    {
                        strArray[index] = string.Copy(validator2.ErrorMessage);
                        index++;
                    }
                }
            }
            return strArray;
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!this.wasForeColorSet && (this.RenderingCompatibility < VersionUtil.Framework40))
            {
                this.ForeColor = Color.Red;
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.Enabled)
            {
                Page page = this.Page;
                if ((page != null) && (page.RequestInternal != null))
                {
                    this.renderUplevel = (this.EnableClientScript && (page.Request.Browser.W3CDomVersion.Major >= 1)) && (page.Request.Browser.EcmaScriptVersion.CompareTo(new Version(1, 2)) >= 0);
                }
                if (this.renderUplevel)
                {
                    string arrayValue = "document.getElementById(\"" + this.ClientID + "\")";
                    if (!this.Page.IsPartialRenderingSupported)
                    {
                        this.Page.ClientScript.RegisterArrayDeclaration("Page_ValidationSummaries", arrayValue);
                    }
                    else
                    {
                        ValidatorCompatibilityHelper.RegisterArrayDeclaration(this, "Page_ValidationSummaries", arrayValue);
                        ValidatorCompatibilityHelper.RegisterStartupScript(this, typeof(ValidationSummary), this.ClientID + "_DisposeScript", string.Format(CultureInfo.InvariantCulture, "\r\ndocument.getElementById('{0}').dispose = function() {{\r\n    Array.remove({1}, document.getElementById('{0}'));\r\n}}\r\n", new object[] { this.ClientID, "Page_ValidationSummaries" }), true);
                    }
                }
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            string[] errorMessages;
            bool flag;
            if (base.DesignMode)
            {
                errorMessages = new string[] { System.Web.SR.GetString("ValSummary_error_message_1"), System.Web.SR.GetString("ValSummary_error_message_2") };
                flag = true;
                this.renderUplevel = false;
            }
            else
            {
                bool flag2;
                if (!this.Enabled)
                {
                    return;
                }
                errorMessages = this.GetErrorMessages(out flag2);
                flag = this.ShowSummary && flag2;
                if (!flag && this.renderUplevel)
                {
                    base.Style["display"] = "none";
                }
            }
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            bool flag3 = this.renderUplevel || flag;
            if (flag3)
            {
                this.RenderBeginTag(writer);
            }
            if (flag)
            {
                string str;
                string str2;
                string str3;
                string str4;
                string str5;
                switch (this.DisplayMode)
                {
                    case ValidationSummaryDisplayMode.List:
                        str = "b";
                        str2 = string.Empty;
                        str3 = string.Empty;
                        str4 = "b";
                        str5 = string.Empty;
                        break;

                    case ValidationSummaryDisplayMode.SingleParagraph:
                        str = " ";
                        str2 = string.Empty;
                        str3 = string.Empty;
                        str4 = " ";
                        str5 = "b";
                        break;

                    default:
                        str = string.Empty;
                        str2 = "<ul>";
                        str3 = "<li>";
                        str4 = "</li>";
                        str5 = "</ul>";
                        break;
                }
                if (this.HeaderText.Length > 0)
                {
                    writer.Write(this.HeaderText);
                    this.WriteBreakIfPresent(writer, str);
                }
                if (errorMessages != null)
                {
                    writer.Write(str2);
                    for (int i = 0; i < errorMessages.Length; i++)
                    {
                        writer.Write(str3);
                        writer.Write(errorMessages[i]);
                        this.WriteBreakIfPresent(writer, str4);
                    }
                    this.WriteBreakIfPresent(writer, str5);
                }
            }
            if (flag3)
            {
                this.RenderEndTag(writer);
            }
        }

        internal bool ShouldSerializeForeColor()
        {
            Color color = (this.RenderingCompatibility < VersionUtil.Framework40) ? Color.Red : Color.Empty;
            return (color != this.ForeColor);
        }

        private void WriteBreakIfPresent(HtmlTextWriter writer, string text)
        {
            if (text == "b")
            {
                if (base.EnableLegacyRendering)
                {
                    writer.WriteObsoleteBreak();
                }
                else
                {
                    writer.WriteBreak();
                }
            }
            else
            {
                writer.Write(text);
            }
        }

        [WebCategory("Appearance"), DefaultValue(1), WebSysDescription("ValidationSummary_DisplayMode")]
        public ValidationSummaryDisplayMode DisplayMode
        {
            get
            {
                object obj2 = this.ViewState["DisplayMode"];
                if (obj2 != null)
                {
                    return (ValidationSummaryDisplayMode) obj2;
                }
                return ValidationSummaryDisplayMode.BulletList;
            }
            set
            {
                if ((value < ValidationSummaryDisplayMode.List) || (value > ValidationSummaryDisplayMode.SingleParagraph))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["DisplayMode"] = value;
            }
        }

        [DefaultValue(true), WebCategory("Behavior"), Themeable(false), WebSysDescription("ValidationSummary_EnableClientScript")]
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

        [WebCategory("Appearance"), Localizable(true), DefaultValue(""), WebSysDescription("ValidationSummary_HeaderText")]
        public string HeaderText
        {
            get
            {
                object obj2 = this.ViewState["HeaderText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["HeaderText"] = value;
            }
        }

        [WebCategory("Behavior"), WebSysDescription("ValidationSummary_ShowMessageBox"), DefaultValue(false)]
        public bool ShowMessageBox
        {
            get
            {
                object obj2 = this.ViewState["ShowMessageBox"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["ShowMessageBox"] = value;
            }
        }

        [WebSysDescription("ValidationSummary_ShowSummary"), WebCategory("Behavior"), DefaultValue(true)]
        public bool ShowSummary
        {
            get
            {
                object obj2 = this.ViewState["ShowSummary"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["ShowSummary"] = value;
            }
        }

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return (this.RenderingCompatibility < VersionUtil.Framework40);
            }
        }

        [WebCategory("Behavior"), Themeable(false), DefaultValue(""), WebSysDescription("ValidationSummary_ValidationGroup")]
        public virtual string ValidationGroup
        {
            get
            {
                string str = (string) this.ViewState["ValidationGroup"];
                if (str != null)
                {
                    return str;
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

