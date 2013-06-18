namespace System.Web.Management
{
    using System;
    using System.Globalization;
    using System.Web;

    internal class TemplatedMailCompileErrorFormatter : DynamicCompileErrorFormatter
    {
        private int _eventsRemaining;
        private bool _showDetails;

        internal TemplatedMailCompileErrorFormatter(HttpCompileException e, int eventsRemaining, bool showDetails) : base(e)
        {
            this._eventsRemaining = eventsRemaining;
            this._showDetails = showDetails;
            base._hideDetailedCompilerOutput = true;
            base._dontShowVersion = true;
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
                return System.Web.SR.GetString("MailWebEventProvider_template_compile_error", new object[] { this._eventsRemaining.ToString(CultureInfo.InstalledUICulture) });
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

