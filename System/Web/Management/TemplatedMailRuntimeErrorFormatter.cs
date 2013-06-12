namespace System.Web.Management
{
    using System;
    using System.Globalization;
    using System.Web;

    internal class TemplatedMailRuntimeErrorFormatter : UnhandledErrorFormatter
    {
        private int _eventsRemaining;
        private bool _showDetails;

        internal TemplatedMailRuntimeErrorFormatter(Exception e, int eventsRemaining, bool showDetails) : base(e)
        {
            this._eventsRemaining = eventsRemaining;
            this._showDetails = showDetails;
            base._dontShowVersion = true;
        }

        protected override string ColoredSquare2Content
        {
            get
            {
                if (this._showDetails)
                {
                    return base.ColoredSquare2Content;
                }
                return null;
            }
        }

        protected override string ColoredSquare2Title
        {
            get
            {
                if (this._showDetails)
                {
                    return base.ColoredSquare2Title;
                }
                return null;
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
                if (this._showDetails)
                {
                    return base.Description;
                }
                return System.Web.SR.GetString("MailWebEventProvider_template_error_no_details");
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                if (HttpException.GetHttpCodeForException(this.Exception) == 0x194)
                {
                    return System.Web.SR.GetString("MailWebEventProvider_template_file_not_found_error", new object[] { this._eventsRemaining.ToString(CultureInfo.InstalledUICulture) });
                }
                return System.Web.SR.GetString("MailWebEventProvider_template_runtime_error", new object[] { this._eventsRemaining.ToString(CultureInfo.InstalledUICulture) });
            }
        }

        protected override string MiscSectionContent
        {
            get
            {
                if (this._showDetails)
                {
                    return base.MiscSectionContent;
                }
                return null;
            }
        }

        protected override string MiscSectionTitle
        {
            get
            {
                if (this._showDetails)
                {
                    return base.MiscSectionTitle;
                }
                return null;
            }
        }
    }
}

