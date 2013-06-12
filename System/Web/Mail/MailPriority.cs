namespace System.Web.Mail
{
    using System;

    [Obsolete("The recommended alternative is System.Net.Mail.MailPriority. http://go.microsoft.com/fwlink/?linkid=14202")]
    public enum MailPriority
    {
        Normal,
        Low,
        High
    }
}

