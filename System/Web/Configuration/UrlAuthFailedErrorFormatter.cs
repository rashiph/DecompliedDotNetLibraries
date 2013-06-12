namespace System.Web.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.Web;

    internal class UrlAuthFailedErrorFormatter : ErrorFormatter
    {
        private StringCollection _adaptiveMiscContent = new StringCollection();

        internal UrlAuthFailedErrorFormatter()
        {
        }

        internal static string GetErrorText()
        {
            return GetErrorText(HttpContext.Current);
        }

        internal static string GetErrorText(HttpContext context)
        {
            bool isCustomErrorEnabled = context.IsCustomErrorEnabled;
            return new UrlAuthFailedErrorFormatter().GetErrorMessage(context, isCustomErrorEnabled);
        }

        protected override StringCollection AdaptiveMiscContent
        {
            get
            {
                return this._adaptiveMiscContent;
            }
        }

        protected override string ColoredSquareContent
        {
            get
            {
                return null;
            }
        }

        protected override string ColoredSquareTitle
        {
            get
            {
                return null;
            }
        }

        protected override string Description
        {
            get
            {
                return System.Web.SR.GetString("Assess_Denied_Description2");
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                return System.Web.SR.GetString("Assess_Denied_Title");
            }
        }

        protected override string MiscSectionContent
        {
            get
            {
                string str = HttpUtility.FormatPlainTextAsHtml(System.Web.SR.GetString("Assess_Denied_Misc_Content2"));
                this.AdaptiveMiscContent.Add(str);
                return str;
            }
        }

        protected override string MiscSectionTitle
        {
            get
            {
                return System.Web.SR.GetString("Assess_Denied_Section_Title2");
            }
        }

        protected override bool ShowSourceFileInfo
        {
            get
            {
                return false;
            }
        }
    }
}

