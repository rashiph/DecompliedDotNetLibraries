namespace System.Web.Security
{
    using System;
    using System.Web;

    internal class FileAccessFailedErrorFormatter : ErrorFormatter
    {
        private string _strFile;

        internal FileAccessFailedErrorFormatter(string strFile)
        {
            this._strFile = strFile;
            if (this._strFile == null)
            {
                this._strFile = string.Empty;
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
                return System.Web.SR.GetString("Assess_Denied_Description3");
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
                string str;
                if (this._strFile.Length > 0)
                {
                    str = System.Web.SR.GetString("Assess_Denied_Misc_Content3", new object[] { HttpRuntime.GetSafePath(this._strFile) });
                }
                else
                {
                    str = System.Web.SR.GetString("Assess_Denied_Misc_Content3_2");
                }
                this.AdaptiveMiscContent.Add(str);
                return str;
            }
        }

        protected override string MiscSectionTitle
        {
            get
            {
                return System.Web.SR.GetString("Assess_Denied_Section_Title3");
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

