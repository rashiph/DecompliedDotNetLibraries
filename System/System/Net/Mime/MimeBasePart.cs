namespace System.Net.Mime
{
    using System;
    using System.Collections.Specialized;
    using System.Net;
    using System.Net.Mail;
    using System.Text;

    internal class MimeBasePart
    {
        protected ContentDisposition contentDisposition;
        protected System.Net.Mime.ContentType contentType;
        internal const string defaultCharSet = "utf-8";
        private HeaderCollection headers;

        internal MimeBasePart()
        {
        }

        internal virtual IAsyncResult BeginSend(BaseWriter writer, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        internal static Encoding DecodeEncoding(string value)
        {
            if ((value == null) || (value.Length == 0))
            {
                return null;
            }
            string[] strArray = value.Split(new char[] { '?' });
            if (((strArray.Length != 5) || (strArray[0] != "=")) || (strArray[4] != "="))
            {
                return null;
            }
            string name = strArray[1];
            return Encoding.GetEncoding(name);
        }

        internal static string DecodeHeaderValue(string value)
        {
            if ((value == null) || (value.Length == 0))
            {
                return string.Empty;
            }
            string str = string.Empty;
            foreach (string str2 in value.Split(new char[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] strArray2 = str2.Split(new char[] { '?' });
                if (((strArray2.Length != 5) || (strArray2[0] != "=")) || (strArray2[4] != "="))
                {
                    return value;
                }
                string name = strArray2[1];
                bool flag = strArray2[2] == "B";
                byte[] bytes = Encoding.ASCII.GetBytes(strArray2[3]);
                EncodedStreamFactory factory = new EncodedStreamFactory();
                int count = factory.GetEncoderForHeader(Encoding.GetEncoding(name), flag, 0).DecodeBytes(bytes, 0, bytes.Length);
                Encoding encoding = Encoding.GetEncoding(name);
                str = str + encoding.GetString(bytes, 0, count);
            }
            return str;
        }

        internal static string EncodeHeaderValue(string value, Encoding encoding, bool base64Encoding)
        {
            return EncodeHeaderValue(value, encoding, base64Encoding, 0);
        }

        internal static string EncodeHeaderValue(string value, Encoding encoding, bool base64Encoding, int headerLength)
        {
            new StringBuilder();
            if (IsAscii(value, false))
            {
                return value;
            }
            if (encoding == null)
            {
                encoding = Encoding.GetEncoding("utf-8");
            }
            IEncodableStream stream = new EncodedStreamFactory().GetEncoderForHeader(encoding, base64Encoding, headerLength);
            byte[] bytes = encoding.GetBytes(value);
            stream.EncodeBytes(bytes, 0, bytes.Length);
            return stream.GetEncodedString();
        }

        internal void EndSend(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            LazyAsyncResult result = asyncResult as MimePartAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
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

        internal static bool IsAnsi(string value, bool permitCROrLF)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            foreach (char ch in value)
            {
                if (ch > '\x00ff')
                {
                    return false;
                }
                if (!permitCROrLF && ((ch == '\r') || (ch == '\n')))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsAscii(string value, bool permitCROrLF)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            foreach (char ch in value)
            {
                if (ch > '\x007f')
                {
                    return false;
                }
                if (!permitCROrLF && ((ch == '\r') || (ch == '\n')))
                {
                    return false;
                }
            }
            return true;
        }

        internal virtual void Send(BaseWriter writer)
        {
            throw new NotImplementedException();
        }

        internal static bool ShouldUseBase64Encoding(Encoding encoding)
        {
            if (((encoding != Encoding.Unicode) && (encoding != Encoding.UTF8)) && ((encoding != Encoding.UTF32) && (encoding != Encoding.BigEndianUnicode)))
            {
                return false;
            }
            return true;
        }

        internal string ContentID
        {
            get
            {
                return this.Headers[MailHeaderInfo.GetString(MailHeaderID.ContentID)];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.ContentID));
                }
                else
                {
                    this.Headers[MailHeaderInfo.GetString(MailHeaderID.ContentID)] = value;
                }
            }
        }

        internal string ContentLocation
        {
            get
            {
                return this.Headers[MailHeaderInfo.GetString(MailHeaderID.ContentLocation)];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.ContentLocation));
                }
                else
                {
                    this.Headers[MailHeaderInfo.GetString(MailHeaderID.ContentLocation)] = value;
                }
            }
        }

        internal System.Net.Mime.ContentType ContentType
        {
            get
            {
                if (this.contentType == null)
                {
                    this.contentType = new System.Net.Mime.ContentType();
                }
                return this.contentType;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.contentType = value;
                this.contentType.PersistIfNeeded((HeaderCollection) this.Headers, true);
            }
        }

        internal NameValueCollection Headers
        {
            get
            {
                if (this.headers == null)
                {
                    this.headers = new HeaderCollection();
                }
                if (this.contentType == null)
                {
                    this.contentType = new System.Net.Mime.ContentType();
                }
                this.contentType.PersistIfNeeded(this.headers, false);
                if (this.contentDisposition != null)
                {
                    this.contentDisposition.PersistIfNeeded(this.headers, false);
                }
                return this.headers;
            }
        }

        internal class MimePartAsyncResult : LazyAsyncResult
        {
            internal MimePartAsyncResult(MimeBasePart part, object state, AsyncCallback callback) : base(part, state, callback)
            {
            }
        }
    }
}

