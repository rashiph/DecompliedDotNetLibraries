namespace System.Web
{
    using System;
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;

    internal class PageForbiddenErrorFormatter : ErrorFormatter
    {
        private StringCollection _adaptiveMiscContent;
        private string _description;
        protected string _htmlEncodedUrl;

        internal PageForbiddenErrorFormatter(string url) : this(url, null)
        {
        }

        internal PageForbiddenErrorFormatter(string url, string description)
        {
            this._adaptiveMiscContent = new StringCollection();
            this._htmlEncodedUrl = HttpUtility.HtmlEncode(url);
            this._adaptiveMiscContent.Add(this._htmlEncodedUrl);
            this._description = description;
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
                if (this._description != null)
                {
                    return this._description;
                }
                Match match = Regex.Match(this._htmlEncodedUrl, @"\.\w+$");
                string str = string.Empty;
                if (match.Success)
                {
                    str = System.Web.SR.GetString("Forbidden_Extension_Incorrect", new object[] { match.ToString() });
                }
                return HttpUtility.FormatPlainTextAsHtml(System.Web.SR.GetString("Forbidden_Extension_Desc", new object[] { str }));
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                return System.Web.SR.GetString("Forbidden_Type_Not_Served");
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

