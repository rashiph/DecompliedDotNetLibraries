namespace System.Net.Configuration
{
    using System;

    internal sealed class MailSettingsSectionGroupInternal
    {
        private SmtpSectionInternal smtp = SmtpSectionInternal.GetSection();

        internal MailSettingsSectionGroupInternal()
        {
        }

        internal static MailSettingsSectionGroupInternal GetSection()
        {
            return new MailSettingsSectionGroupInternal();
        }

        internal SmtpSectionInternal Smtp
        {
            get
            {
                return this.smtp;
            }
        }
    }
}

