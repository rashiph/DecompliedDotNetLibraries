namespace System.Net.Mail
{
    using System;
    using System.Collections.Specialized;
    using System.Net;
    using System.Net.Mime;
    using System.Text;

    public class MailMessage : IDisposable
    {
        private AttachmentCollection attachments;
        private string body;
        private Encoding bodyEncoding;
        private AlternateView bodyView;
        private System.Net.Mail.DeliveryNotificationOptions deliveryStatusNotification;
        private bool disposed;
        private bool isBodyHtml;
        private Message message;
        private AlternateViewCollection views;

        public MailMessage()
        {
            this.body = string.Empty;
            this.message = new Message();
            if (Logging.On)
            {
                Logging.Associate(Logging.Web, this, this.message);
            }
            string from = SmtpClient.MailConfiguration.Smtp.From;
            if ((from != null) && (from.Length > 0))
            {
                this.message.From = new MailAddress(from);
            }
        }

        public MailMessage(MailAddress from, MailAddress to)
        {
            this.body = string.Empty;
            if (from == null)
            {
                throw new ArgumentNullException("from");
            }
            if (to == null)
            {
                throw new ArgumentNullException("to");
            }
            this.message = new Message(from, to);
        }

        public MailMessage(string from, string to)
        {
            this.body = string.Empty;
            if (from == null)
            {
                throw new ArgumentNullException("from");
            }
            if (to == null)
            {
                throw new ArgumentNullException("to");
            }
            if (from == string.Empty)
            {
                throw new ArgumentException(SR.GetString("net_emptystringcall", new object[] { "from" }), "from");
            }
            if (to == string.Empty)
            {
                throw new ArgumentException(SR.GetString("net_emptystringcall", new object[] { "to" }), "to");
            }
            this.message = new Message(from, to);
            if (Logging.On)
            {
                Logging.Associate(Logging.Web, this, this.message);
            }
        }

        public MailMessage(string from, string to, string subject, string body) : this(from, to)
        {
            this.Subject = subject;
            this.Body = body;
        }

        internal IAsyncResult BeginSend(BaseWriter writer, bool sendEnvelope, AsyncCallback callback, object state)
        {
            this.SetContent();
            return this.message.BeginSend(writer, sendEnvelope, callback, state);
        }

        internal string BuildDeliveryStatusNotificationString()
        {
            if (this.deliveryStatusNotification == System.Net.Mail.DeliveryNotificationOptions.None)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(" NOTIFY=");
            bool flag = false;
            if (this.deliveryStatusNotification == System.Net.Mail.DeliveryNotificationOptions.Never)
            {
                builder.Append("NEVER");
                return builder.ToString();
            }
            if ((this.deliveryStatusNotification & System.Net.Mail.DeliveryNotificationOptions.OnSuccess) > System.Net.Mail.DeliveryNotificationOptions.None)
            {
                builder.Append("SUCCESS");
                flag = true;
            }
            if ((this.deliveryStatusNotification & System.Net.Mail.DeliveryNotificationOptions.OnFailure) > System.Net.Mail.DeliveryNotificationOptions.None)
            {
                if (flag)
                {
                    builder.Append(",");
                }
                builder.Append("FAILURE");
                flag = true;
            }
            if ((this.deliveryStatusNotification & System.Net.Mail.DeliveryNotificationOptions.Delay) > System.Net.Mail.DeliveryNotificationOptions.None)
            {
                if (flag)
                {
                    builder.Append(",");
                }
                builder.Append("DELAY");
            }
            return builder.ToString();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this.disposed)
            {
                this.disposed = true;
                if (this.views != null)
                {
                    this.views.Dispose();
                }
                if (this.attachments != null)
                {
                    this.attachments.Dispose();
                }
                if (this.bodyView != null)
                {
                    this.bodyView.Dispose();
                }
            }
        }

        internal void EndSend(IAsyncResult asyncResult)
        {
            this.message.EndSend(asyncResult);
        }

        internal void Send(BaseWriter writer, bool sendEnvelope)
        {
            this.SetContent();
            this.message.Send(writer, sendEnvelope);
        }

        private void SetContent()
        {
            if (this.bodyView != null)
            {
                this.bodyView.Dispose();
                this.bodyView = null;
            }
            if ((this.AlternateViews.Count == 0) && (this.Attachments.Count == 0))
            {
                if ((this.body != null) && (this.body != string.Empty))
                {
                    this.bodyView = AlternateView.CreateAlternateViewFromString(this.body, this.bodyEncoding, this.isBodyHtml ? "text/html" : null);
                    this.message.Content = this.bodyView.MimePart;
                }
            }
            else if ((this.AlternateViews.Count == 0) && (this.Attachments.Count > 0))
            {
                MimeMultiPart part = new MimeMultiPart(MimeMultiPartType.Mixed);
                if ((this.body != null) && (this.body != string.Empty))
                {
                    this.bodyView = AlternateView.CreateAlternateViewFromString(this.body, this.bodyEncoding, this.isBodyHtml ? "text/html" : null);
                }
                else
                {
                    this.bodyView = AlternateView.CreateAlternateViewFromString(string.Empty);
                }
                part.Parts.Add(this.bodyView.MimePart);
                foreach (Attachment attachment in this.Attachments)
                {
                    if (attachment != null)
                    {
                        attachment.PrepareForSending();
                        part.Parts.Add(attachment.MimePart);
                    }
                }
                this.message.Content = part;
            }
            else
            {
                MimeMultiPart part2 = null;
                MimeMultiPart part3 = new MimeMultiPart(MimeMultiPartType.Alternative);
                if ((this.body != null) && (this.body != string.Empty))
                {
                    this.bodyView = AlternateView.CreateAlternateViewFromString(this.body, this.bodyEncoding, null);
                    part3.Parts.Add(this.bodyView.MimePart);
                }
                foreach (AlternateView view in this.AlternateViews)
                {
                    if (view != null)
                    {
                        view.PrepareForSending();
                        if (view.LinkedResources.Count > 0)
                        {
                            MimeMultiPart item = new MimeMultiPart(MimeMultiPartType.Related);
                            item.ContentType.Parameters["type"] = view.ContentType.MediaType;
                            item.ContentLocation = view.MimePart.ContentLocation;
                            item.Parts.Add(view.MimePart);
                            foreach (LinkedResource resource in view.LinkedResources)
                            {
                                resource.PrepareForSending();
                                item.Parts.Add(resource.MimePart);
                            }
                            part3.Parts.Add(item);
                        }
                        else
                        {
                            part3.Parts.Add(view.MimePart);
                        }
                    }
                }
                if (this.Attachments.Count > 0)
                {
                    part2 = new MimeMultiPart(MimeMultiPartType.Mixed) {
                        Parts = { part3 }
                    };
                    MimeMultiPart part5 = new MimeMultiPart(MimeMultiPartType.Mixed);
                    foreach (Attachment attachment2 in this.Attachments)
                    {
                        if (attachment2 != null)
                        {
                            attachment2.PrepareForSending();
                            part5.Parts.Add(attachment2.MimePart);
                        }
                    }
                    part2.Parts.Add(part5);
                    this.message.Content = part2;
                }
                else if ((part3.Parts.Count == 1) && ((this.body == null) || (this.body == string.Empty)))
                {
                    this.message.Content = part3.Parts[0];
                }
                else
                {
                    this.message.Content = part3;
                }
            }
        }

        public AlternateViewCollection AlternateViews
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                if (this.views == null)
                {
                    this.views = new AlternateViewCollection();
                }
                return this.views;
            }
        }

        public AttachmentCollection Attachments
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                if (this.attachments == null)
                {
                    this.attachments = new AttachmentCollection();
                }
                return this.attachments;
            }
        }

        public MailAddressCollection Bcc
        {
            get
            {
                return this.message.Bcc;
            }
        }

        public string Body
        {
            get
            {
                if (this.body == null)
                {
                    return string.Empty;
                }
                return this.body;
            }
            set
            {
                this.body = value;
                if ((this.bodyEncoding == null) && (this.body != null))
                {
                    if (MimeBasePart.IsAscii(this.body, true))
                    {
                        this.bodyEncoding = Encoding.ASCII;
                    }
                    else
                    {
                        this.bodyEncoding = Encoding.GetEncoding("utf-8");
                    }
                }
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

        public MailAddressCollection CC
        {
            get
            {
                return this.message.CC;
            }
        }

        public System.Net.Mail.DeliveryNotificationOptions DeliveryNotificationOptions
        {
            get
            {
                return this.deliveryStatusNotification;
            }
            set
            {
                if (((System.Net.Mail.DeliveryNotificationOptions.Delay | System.Net.Mail.DeliveryNotificationOptions.OnFailure | System.Net.Mail.DeliveryNotificationOptions.OnSuccess) < value) && (value != System.Net.Mail.DeliveryNotificationOptions.Never))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.deliveryStatusNotification = value;
            }
        }

        public MailAddress From
        {
            get
            {
                return this.message.From;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.message.From = value;
            }
        }

        public NameValueCollection Headers
        {
            get
            {
                return this.message.Headers;
            }
        }

        public Encoding HeadersEncoding
        {
            get
            {
                return this.message.HeadersEncoding;
            }
            set
            {
                this.message.HeadersEncoding = value;
            }
        }

        public bool IsBodyHtml
        {
            get
            {
                return this.isBodyHtml;
            }
            set
            {
                this.isBodyHtml = value;
            }
        }

        public MailPriority Priority
        {
            get
            {
                return this.message.Priority;
            }
            set
            {
                this.message.Priority = value;
            }
        }

        [Obsolete("ReplyTo is obsoleted for this type.  Please use ReplyToList instead which can accept multiple addresses. http://go.microsoft.com/fwlink/?linkid=14202")]
        public MailAddress ReplyTo
        {
            get
            {
                return this.message.ReplyTo;
            }
            set
            {
                this.message.ReplyTo = value;
            }
        }

        public MailAddressCollection ReplyToList
        {
            get
            {
                return this.message.ReplyToList;
            }
        }

        public MailAddress Sender
        {
            get
            {
                return this.message.Sender;
            }
            set
            {
                this.message.Sender = value;
            }
        }

        public string Subject
        {
            get
            {
                if (this.message.Subject == null)
                {
                    return string.Empty;
                }
                return this.message.Subject;
            }
            set
            {
                this.message.Subject = value;
            }
        }

        public Encoding SubjectEncoding
        {
            get
            {
                return this.message.SubjectEncoding;
            }
            set
            {
                this.message.SubjectEncoding = value;
            }
        }

        public MailAddressCollection To
        {
            get
            {
                return this.message.To;
            }
        }
    }
}

