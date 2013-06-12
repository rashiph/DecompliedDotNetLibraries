namespace System.Web.Mail
{
    using System;

    [Obsolete("The recommended alternative is System.Net.Mime.TransferEncoding. http://go.microsoft.com/fwlink/?linkid=14202")]
    public enum MailEncoding
    {
        UUEncode,
        Base64
    }
}

