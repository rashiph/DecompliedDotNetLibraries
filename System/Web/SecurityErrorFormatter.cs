namespace System.Web
{
    using System;

    internal class SecurityErrorFormatter : UnhandledErrorFormatter
    {
        internal SecurityErrorFormatter(Exception e) : base(e)
        {
        }

        protected override string Description
        {
            get
            {
                return HttpUtility.FormatPlainTextAsHtml(System.Web.SR.GetString("Security_Err_Desc"));
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                return System.Web.SR.GetString("Security_Err_Error");
            }
        }
    }
}

