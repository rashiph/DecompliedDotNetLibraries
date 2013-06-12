namespace System.Web.Handlers
{
    using System;
    using System.Web;

    internal class TraceHandlerErrorFormatter : ErrorFormatter
    {
        private bool _isRemote;

        internal TraceHandlerErrorFormatter(bool isRemote)
        {
            this._isRemote = isRemote;
        }

        internal override bool CanBeShownToAllUsers
        {
            get
            {
                return true;
            }
        }

        protected override string ColoredSquareContent
        {
            get
            {
                string str;
                if (this._isRemote)
                {
                    str = HttpUtility.HtmlEncode(System.Web.SR.GetString("Trace_Error_LocalOnly_Details_Sample"));
                }
                else
                {
                    str = HttpUtility.HtmlEncode(System.Web.SR.GetString("Trace_Error_Enabled_Details_Sample"));
                }
                return base.WrapWithLeftToRightTextFormatIfNeeded(str);
            }
        }

        protected override string ColoredSquareDescription
        {
            get
            {
                string str;
                if (this._isRemote)
                {
                    str = HttpUtility.HtmlEncode(System.Web.SR.GetString("Trace_Error_LocalOnly_Details_Desc"));
                }
                else
                {
                    str = HttpUtility.HtmlEncode(System.Web.SR.GetString("Trace_Error_Enabled_Details_Desc"));
                }
                this.AdaptiveMiscContent.Add(str);
                return str;
            }
        }

        protected override string ColoredSquareTitle
        {
            get
            {
                string str = System.Web.SR.GetString("Generic_Err_Details_Title");
                this.AdaptiveMiscContent.Add(str);
                return str;
            }
        }

        protected override string Description
        {
            get
            {
                if (this._isRemote)
                {
                    return System.Web.SR.GetString("Trace_Error_LocalOnly_Description");
                }
                return HttpUtility.HtmlEncode(System.Web.SR.GetString("Trace_Error_Enabled_Description"));
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                return System.Web.SR.GetString("Trace_Error_Title");
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

