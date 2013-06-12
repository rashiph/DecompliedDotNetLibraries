namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [Designer("System.Web.UI.Design.WebControls.LoginNameDesigner,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("FormatString"), Bindable(false)]
    public class LoginName : WebControl
    {
        private const string _defaultFormatString = "{0}";

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (!string.IsNullOrEmpty(this.UserName))
            {
                base.Render(writer);
            }
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            if (!string.IsNullOrEmpty(this.UserName))
            {
                base.RenderBeginTag(writer);
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            string userName = this.UserName;
            if (!string.IsNullOrEmpty(userName))
            {
                userName = HttpUtility.HtmlEncode(userName);
                string formatString = this.FormatString;
                if (formatString.Length == 0)
                {
                    writer.Write(userName);
                }
                else
                {
                    try
                    {
                        writer.Write(string.Format(CultureInfo.CurrentCulture, formatString, new object[] { userName }));
                    }
                    catch (FormatException exception)
                    {
                        throw new FormatException(System.Web.SR.GetString("LoginName_InvalidFormatString"), exception);
                    }
                }
            }
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            if (!string.IsNullOrEmpty(this.UserName))
            {
                base.RenderEndTag(writer);
            }
        }

        [DefaultValue("{0}"), WebCategory("Appearance"), Localizable(true), WebSysDescription("LoginName_FormatString")]
        public virtual string FormatString
        {
            get
            {
                object obj2 = this.ViewState["FormatString"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return "{0}";
            }
            set
            {
                this.ViewState["FormatString"] = value;
            }
        }

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return (this.RenderingCompatibility < VersionUtil.Framework40);
            }
        }

        internal string UserName
        {
            get
            {
                if (base.DesignMode)
                {
                    return System.Web.SR.GetString("LoginName_DesignModeUserName");
                }
                return LoginUtil.GetUserName(this);
            }
        }
    }
}

