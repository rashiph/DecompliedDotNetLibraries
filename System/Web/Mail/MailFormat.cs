namespace System.Web.Mail
{
    using System;

    [Obsolete("The recommended alternative is System.Net.Mail.MailMessage.IsBodyHtml. http://go.microsoft.com/fwlink/?linkid=14202")]
    public enum MailFormat
    {
        Text,
        Html
    }
}

