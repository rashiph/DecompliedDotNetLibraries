namespace System.Web.Mail
{
    using System;
    using System.IO;
    using System.Web;

    [Obsolete("The recommended alternative is System.Net.Mail.Attachment. http://go.microsoft.com/fwlink/?linkid=14202")]
    public class MailAttachment
    {
        private MailEncoding _encoding;
        private string _filename;

        public MailAttachment(string filename)
        {
            this._filename = filename;
            this._encoding = MailEncoding.Base64;
            this.VerifyFile();
        }

        public MailAttachment(string filename, MailEncoding encoding)
        {
            this._filename = filename;
            this._encoding = encoding;
            this.VerifyFile();
        }

        private void VerifyFile()
        {
            try
            {
                File.Open(this._filename, FileMode.Open, FileAccess.Read, FileShare.Read).Close();
            }
            catch
            {
                throw new HttpException(System.Web.SR.GetString("Bad_attachment", new object[] { this._filename }));
            }
        }

        public MailEncoding Encoding
        {
            get
            {
                return this._encoding;
            }
        }

        public string Filename
        {
            get
            {
                return this._filename;
            }
        }
    }
}

