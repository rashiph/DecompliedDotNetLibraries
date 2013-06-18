namespace System.Web
{
    using System;
    using System.Collections.Specialized;

    internal class PageNotFoundErrorFormatter : ErrorFormatter
    {
        private StringCollection _adaptiveMiscContent = new StringCollection();
        protected string _htmlEncodedUrl;

        internal PageNotFoundErrorFormatter(string url)
        {
            this._htmlEncodedUrl = HttpUtility.HtmlEncode(url);
            this._adaptiveMiscContent.Add(this._htmlEncodedUrl);
        }

        protected override StringCollection AdaptiveMiscContent
        {
            get
            {
                return this._adaptiveMiscContent;
            }
        }

        internal override bool CanBeShownToAllUsers
        {
            get
            {
                return true;
            }
        }

        protected override string Description
        {
            get
            {
                return HttpUtility.FormatPlainTextAsHtml(System.Web.SR.GetString("NotFound_Http_404"));
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                return System.Web.SR.GetString("NotFound_Resource_Not_Found");
            }
        }

        protected override string MiscSectionContent
        {
            get
            {
                return this._htmlEncodedUrl;
            }
        }

        protected override string MiscSectionTitle
        {
            get
            {
                return System.Web.SR.GetString("NotFound_Requested_Url");
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

