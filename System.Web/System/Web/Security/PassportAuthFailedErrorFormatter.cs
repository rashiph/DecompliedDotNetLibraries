namespace System.Web.Security
{
    using System;
    using System.Web;

    internal class PassportAuthFailedErrorFormatter : ErrorFormatter
    {
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
                return System.Web.SR.GetString("PassportAuthFailed_Description");
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                return System.Web.SR.GetString("PassportAuthFailed_Title");
            }
        }

        protected override string MiscSectionContent
        {
            get
            {
                return null;
            }
        }

        protected override string MiscSectionTitle
        {
            get
            {
                return System.Web.SR.GetString("Assess_Denied_Title");
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

