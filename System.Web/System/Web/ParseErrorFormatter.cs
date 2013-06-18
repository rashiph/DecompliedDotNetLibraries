namespace System.Web
{
    using System;
    using System.Collections.Specialized;

    internal class ParseErrorFormatter : FormatterWithFileInfo
    {
        private StringCollection _adaptiveMiscContent;
        private HttpParseException _excep;
        protected string _message;

        internal ParseErrorFormatter(HttpParseException e, string virtualPath, string sourceCode, int line, string message) : base(virtualPath, null, sourceCode, line)
        {
            this._adaptiveMiscContent = new StringCollection();
            this._excep = e;
            this._message = HttpUtility.FormatPlainTextAsHtml(message);
            this._adaptiveMiscContent.Add(this._message);
        }

        protected override StringCollection AdaptiveMiscContent
        {
            get
            {
                return this._adaptiveMiscContent;
            }
        }

        protected override string ColoredSquareTitle
        {
            get
            {
                return System.Web.SR.GetString("Parser_Source_Error");
            }
        }

        protected override string Description
        {
            get
            {
                return System.Web.SR.GetString("Parser_Desc");
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                return System.Web.SR.GetString("Parser_Error");
            }
        }

        protected override System.Exception Exception
        {
            get
            {
                return this._excep;
            }
        }

        protected override string MiscSectionContent
        {
            get
            {
                return this._message;
            }
        }

        protected override string MiscSectionTitle
        {
            get
            {
                return System.Web.SR.GetString("Parser_Error_Message");
            }
        }
    }
}

