namespace System.Web.Security
{
    using System;
    using System.Web;

    internal sealed class AuthStoreErrorFormatter : ErrorFormatter
    {
        private static string s_errMsg = null;
        private static object s_Lock = new object();

        internal AuthStoreErrorFormatter()
        {
        }

        internal static string GetErrorText()
        {
            if (s_errMsg == null)
            {
                lock (s_Lock)
                {
                    if (s_errMsg != null)
                    {
                        return s_errMsg;
                    }
                    s_errMsg = new AuthStoreErrorFormatter().GetErrorMessage();
                }
            }
            return s_errMsg;
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
                return System.Web.SR.GetString("AuthStoreNotInstalled_Description");
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                return System.Web.SR.GetString("AuthStoreNotInstalled_Title");
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
                return null;
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

