namespace System.Net.Mail
{
    using System;
    using System.Net;
    using System.Net.Mime;
    using System.Text;

    internal class Message
    {
        private MailAddressCollection bcc;
        private MailAddressCollection cc;
        private MimeBasePart content;
        private HeaderCollection envelopeHeaders;
        private MailAddress from;
        private HeaderCollection headers;
        private Encoding headersEncoding;
        private MailPriority priority;
        private MailAddress replyTo;
        private MailAddressCollection replyToList;
        private MailAddress sender;
        private string subject;
        private Encoding subjectEncoding;
        private MailAddressCollection to;

        internal Message()
        {
            this.priority = ~MailPriority.Normal;
        }

        internal Message(MailAddress from, MailAddress to) : this()
        {
            this.from = from;
            this.To.Add(to);
        }

        internal Message(string from, string to) : this()
        {
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
            this.from = new MailAddress(from);
            MailAddressCollection addresss = new MailAddressCollection();
            addresss.Add(to);
            this.to = addresss;
        }

        internal virtual IAsyncResult BeginSend(BaseWriter writer, bool sendEnvelope, AsyncCallback callback, object state)
        {
            this.PrepareHeaders(sendEnvelope);
            writer.WriteHeaders(this.Headers);
            if (this.Content != null)
            {
                return this.Content.BeginSend(writer, callback, state);
            }
            LazyAsyncResult result = new LazyAsyncResult(this, state, callback);
            IAsyncResult result2 = writer.BeginGetContentStream(new AsyncCallback(this.EmptySendCallback), new EmptySendContext(writer, result));
            if (result2.CompletedSynchronously)
            {
                writer.EndGetContentStream(result2).Close();
            }
            return result;
        }

        internal void EmptySendCallback(IAsyncResult result)
        {
            Exception exception = null;
            if (!result.CompletedSynchronously)
            {
                EmptySendContext asyncState = (EmptySendContext) result.AsyncState;
                try
                {
                    asyncState.writer.EndGetContentStream(result).Close();
                }
                catch (Exception exception2)
                {
                    exception = exception2;
                }
                asyncState.result.InvokeCallback(exception);
            }
        }

        internal void EncodeHeaders(HeaderCollection headers)
        {
            if (this.headersEncoding == null)
            {
                this.headersEncoding = Encoding.GetEncoding("utf-8");
            }
            for (int i = 0; i < headers.Count; i++)
            {
                string key = headers.GetKey(i);
                if (MailHeaderInfo.IsUserSettable(key))
                {
                    string[] values = headers.GetValues(key);
                    string str2 = string.Empty;
                    for (int j = 0; j < values.Length; j++)
                    {
                        if (MimeBasePart.IsAscii(values[j], false))
                        {
                            str2 = values[j];
                        }
                        else
                        {
                            str2 = MimeBasePart.EncodeHeaderValue(values[j], this.headersEncoding, MimeBasePart.ShouldUseBase64Encoding(this.headersEncoding), key.Length);
                        }
                        if (j == 0)
                        {
                            headers.Set(key, str2);
                        }
                        else
                        {
                            headers.Add(key, str2);
                        }
                    }
                }
            }
        }

        internal virtual void EndSend(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (this.Content != null)
            {
                this.Content.EndSend(asyncResult);
            }
            else
            {
                LazyAsyncResult result = asyncResult as LazyAsyncResult;
                if ((result == null) || (result.AsyncObject != this))
                {
                    throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"));
                }
                if (result.EndCalled)
                {
                    throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndSend" }));
                }
                result.InternalWaitForCompletion();
                result.EndCalled = true;
                if (result.Result is Exception)
                {
                    throw ((Exception) result.Result);
                }
            }
        }

        private bool IsHeaderSet(string headerName)
        {
            for (int i = 0; i < this.Headers.Count; i++)
            {
                if (string.Compare(this.Headers.GetKey(i), headerName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        internal void PrepareEnvelopeHeaders(bool sendEnvelope)
        {
            if (this.headersEncoding == null)
            {
                this.headersEncoding = Encoding.GetEncoding("utf-8");
            }
            this.EncodeHeaders(this.EnvelopeHeaders);
            string headerName = MailHeaderInfo.GetString(MailHeaderID.XSender);
            if (!this.IsHeaderSet(headerName))
            {
                this.EnvelopeHeaders.InternalSet(headerName, this.From.Encode(headerName.Length));
            }
            this.EnvelopeHeaders.Remove(MailHeaderInfo.GetString(MailHeaderID.XReceiver));
            foreach (MailAddress address in this.To)
            {
                this.EnvelopeHeaders.InternalAdd(MailHeaderInfo.GetString(MailHeaderID.XReceiver), address.Encode(MailHeaderInfo.GetString(MailHeaderID.XReceiver).Length));
            }
            foreach (MailAddress address2 in this.CC)
            {
                this.EnvelopeHeaders.InternalAdd(MailHeaderInfo.GetString(MailHeaderID.XReceiver), address2.Encode(MailHeaderInfo.GetString(MailHeaderID.XReceiver).Length));
            }
            foreach (MailAddress address3 in this.Bcc)
            {
                this.EnvelopeHeaders.InternalAdd(MailHeaderInfo.GetString(MailHeaderID.XReceiver), address3.Encode(MailHeaderInfo.GetString(MailHeaderID.XReceiver).Length));
            }
        }

        internal void PrepareHeaders(bool sendEnvelope)
        {
            if (this.headersEncoding == null)
            {
                this.headersEncoding = Encoding.GetEncoding("utf-8");
            }
            this.Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.ContentType));
            this.Headers[MailHeaderInfo.GetString(MailHeaderID.MimeVersion)] = "1.0";
            if (this.Sender != null)
            {
                this.Headers.InternalAdd(MailHeaderInfo.GetString(MailHeaderID.Sender), this.Sender.Encode(MailHeaderInfo.GetString(MailHeaderID.Sender).ToString().Length));
            }
            else
            {
                this.Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Sender));
            }
            this.Headers.InternalAdd(MailHeaderInfo.GetString(MailHeaderID.From), this.From.Encode(MailHeaderInfo.GetString(MailHeaderID.From).ToString().Length));
            if (this.To.Count > 0)
            {
                string str = this.To.Encode(MailHeaderInfo.GetString(MailHeaderID.To).Length);
                this.Headers.InternalAdd(MailHeaderInfo.GetString(MailHeaderID.To), str);
            }
            else
            {
                this.Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.To));
            }
            if (this.CC.Count > 0)
            {
                this.Headers.InternalAdd(MailHeaderInfo.GetString(MailHeaderID.Cc), this.CC.Encode(MailHeaderInfo.GetString(MailHeaderID.Cc).Length));
            }
            else
            {
                this.Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Cc));
            }
            if (this.ReplyTo != null)
            {
                this.Headers.InternalAdd(MailHeaderInfo.GetString(MailHeaderID.ReplyTo), this.ReplyTo.Encode(MailHeaderInfo.GetString(MailHeaderID.ReplyTo).Length));
            }
            else if (this.ReplyToList.Count > 0)
            {
                this.Headers.InternalAdd(MailHeaderInfo.GetString(MailHeaderID.ReplyTo), this.ReplyToList.Encode(MailHeaderInfo.GetString(MailHeaderID.ReplyTo).Length));
            }
            else
            {
                this.Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.ReplyTo));
            }
            this.Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Bcc));
            if (this.priority == MailPriority.High)
            {
                this.Headers[MailHeaderInfo.GetString(MailHeaderID.XPriority)] = "1";
                this.Headers[MailHeaderInfo.GetString(MailHeaderID.Priority)] = "urgent";
                this.Headers[MailHeaderInfo.GetString(MailHeaderID.Importance)] = "high";
            }
            else if (this.priority == MailPriority.Low)
            {
                this.Headers[MailHeaderInfo.GetString(MailHeaderID.XPriority)] = "5";
                this.Headers[MailHeaderInfo.GetString(MailHeaderID.Priority)] = "non-urgent";
                this.Headers[MailHeaderInfo.GetString(MailHeaderID.Importance)] = "low";
            }
            else if (this.priority != ~MailPriority.Normal)
            {
                this.Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.XPriority));
                this.Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Priority));
                this.Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Importance));
            }
            this.Headers.InternalAdd(MailHeaderInfo.GetString(MailHeaderID.Date), MailBnfHelper.GetDateTimeString(DateTime.Now, null));
            if (!string.IsNullOrEmpty(this.subject))
            {
                this.Headers.InternalAdd(MailHeaderInfo.GetString(MailHeaderID.Subject), MimeBasePart.EncodeHeaderValue(this.subject, this.subjectEncoding, MimeBasePart.ShouldUseBase64Encoding(this.subjectEncoding), MailHeaderID.Subject.ToString().Length));
            }
            else
            {
                this.Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Subject));
            }
            this.EncodeHeaders(this.headers);
        }

        internal virtual void Send(BaseWriter writer, bool sendEnvelope)
        {
            if (sendEnvelope)
            {
                this.PrepareEnvelopeHeaders(sendEnvelope);
                writer.WriteHeaders(this.EnvelopeHeaders);
            }
            this.PrepareHeaders(sendEnvelope);
            writer.WriteHeaders(this.Headers);
            if (this.Content != null)
            {
                this.Content.Send(writer);
            }
            else
            {
                writer.GetContentStream().Close();
            }
        }

        internal MailAddressCollection Bcc
        {
            get
            {
                if (this.bcc == null)
                {
                    this.bcc = new MailAddressCollection();
                }
                return this.bcc;
            }
        }

        internal MailAddressCollection CC
        {
            get
            {
                if (this.cc == null)
                {
                    this.cc = new MailAddressCollection();
                }
                return this.cc;
            }
        }

        internal virtual MimeBasePart Content
        {
            get
            {
                return this.content;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.content = value;
            }
        }

        internal HeaderCollection EnvelopeHeaders
        {
            get
            {
                if (this.envelopeHeaders == null)
                {
                    this.envelopeHeaders = new HeaderCollection();
                    if (Logging.On)
                    {
                        Logging.Associate(Logging.Web, this, this.envelopeHeaders);
                    }
                }
                return this.envelopeHeaders;
            }
        }

        internal MailAddress From
        {
            get
            {
                return this.from;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.from = value;
            }
        }

        internal HeaderCollection Headers
        {
            get
            {
                if (this.headers == null)
                {
                    this.headers = new HeaderCollection();
                    if (Logging.On)
                    {
                        Logging.Associate(Logging.Web, this, this.headers);
                    }
                }
                return this.headers;
            }
        }

        internal Encoding HeadersEncoding
        {
            get
            {
                return this.headersEncoding;
            }
            set
            {
                this.headersEncoding = value;
            }
        }

        public MailPriority Priority
        {
            get
            {
                if (this.priority != ~MailPriority.Normal)
                {
                    return this.priority;
                }
                return MailPriority.Normal;
            }
            set
            {
                this.priority = value;
            }
        }

        internal MailAddress ReplyTo
        {
            get
            {
                return this.replyTo;
            }
            set
            {
                this.replyTo = value;
            }
        }

        internal MailAddressCollection ReplyToList
        {
            get
            {
                if (this.replyToList == null)
                {
                    this.replyToList = new MailAddressCollection();
                }
                return this.replyToList;
            }
        }

        internal MailAddress Sender
        {
            get
            {
                return this.sender;
            }
            set
            {
                this.sender = value;
            }
        }

        internal string Subject
        {
            get
            {
                return this.subject;
            }
            set
            {
                if ((value != null) && MailBnfHelper.HasCROrLF(value))
                {
                    throw new ArgumentException(SR.GetString("MailSubjectInvalidFormat"));
                }
                this.subject = value;
                if (((this.subject != null) && (this.subjectEncoding == null)) && !MimeBasePart.IsAscii(this.subject, false))
                {
                    this.subjectEncoding = Encoding.GetEncoding("utf-8");
                }
            }
        }

        internal Encoding SubjectEncoding
        {
            get
            {
                return this.subjectEncoding;
            }
            set
            {
                this.subjectEncoding = value;
            }
        }

        internal MailAddressCollection To
        {
            get
            {
                if (this.to == null)
                {
                    this.to = new MailAddressCollection();
                }
                return this.to;
            }
        }

        internal class EmptySendContext
        {
            internal LazyAsyncResult result;
            internal BaseWriter writer;

            internal EmptySendContext(BaseWriter writer, LazyAsyncResult result)
            {
                this.writer = writer;
                this.result = result;
            }
        }
    }
}

