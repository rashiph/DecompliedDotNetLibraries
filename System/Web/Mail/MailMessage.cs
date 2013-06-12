namespace System.Web.Mail
{
    using System;
    using System.Collections;
    using System.Text;

    [Obsolete("The recommended alternative is System.Net.Mail.MailMessage. http://go.microsoft.com/fwlink/?linkid=14202")]
    public class MailMessage
    {
        private ArrayList _attachments = new ArrayList();
        private Hashtable _fields = new Hashtable();
        private Hashtable _headers = new Hashtable();
        private string bcc;
        private string body;
        private Encoding bodyEncoding = Encoding.Default;
        private MailFormat bodyFormat;
        private string cc;
        private string from;
        private MailPriority priority;
        private string subject;
        private string to;
        private string urlContentBase;
        private string urlContentLocation;

        public IList Attachments
        {
            get
            {
                return this._attachments;
            }
        }

        public string Bcc
        {
            get
            {
                return this.bcc;
            }
            set
            {
                this.bcc = value;
            }
        }

        public string Body
        {
            get
            {
                return this.body;
            }
            set
            {
                this.body = value;
            }
        }

        public Encoding BodyEncoding
        {
            get
            {
                return this.bodyEncoding;
            }
            set
            {
                this.bodyEncoding = value;
            }
        }

        public MailFormat BodyFormat
        {
            get
            {
                return this.bodyFormat;
            }
            set
            {
                this.bodyFormat = value;
            }
        }

        public string Cc
        {
            get
            {
                return this.cc;
            }
            set
            {
                this.cc = value;
            }
        }

        public IDictionary Fields
        {
            get
            {
                return this._fields;
            }
        }

        public string From
        {
            get
            {
                return this.from;
            }
            set
            {
                this.from = value;
            }
        }

        public IDictionary Headers
        {
            get
            {
                return this._headers;
            }
        }

        public MailPriority Priority
        {
            get
            {
                return this.priority;
            }
            set
            {
                this.priority = value;
            }
        }

        public string Subject
        {
            get
            {
                return this.subject;
            }
            set
            {
                this.subject = value;
            }
        }

        public string To
        {
            get
            {
                return this.to;
            }
            set
            {
                this.to = value;
            }
        }

        public string UrlContentBase
        {
            get
            {
                return this.urlContentBase;
            }
            set
            {
                this.urlContentBase = value;
            }
        }

        public string UrlContentLocation
        {
            get
            {
                return this.urlContentLocation;
            }
            set
            {
                this.urlContentLocation = value;
            }
        }
    }
}

