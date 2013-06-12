namespace System.Web
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Text;

    internal class ConfigErrorFormatter : FormatterWithFileInfo
    {
        private StringCollection _adaptiveMiscContent;
        private System.Exception _e;
        protected string _message;

        internal ConfigErrorFormatter(ConfigurationException e) : base(null, e.Filename, null, e.Line)
        {
            this._adaptiveMiscContent = new StringCollection();
            this._e = e;
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_PRE_PROCESSING);
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);
            this._message = HttpUtility.FormatPlainTextAsHtml(e.BareMessage);
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
                return System.Web.SR.GetString("Config_Desc");
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                return System.Web.SR.GetString("Config_Error");
            }
        }

        protected override System.Exception Exception
        {
            get
            {
                return this._e;
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

        protected override Encoding SourceFileEncoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
    }
}

