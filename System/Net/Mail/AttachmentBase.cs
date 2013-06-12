namespace System.Net.Mail
{
    using System;
    using System.IO;
    using System.Net.Mime;
    using System.Text;

    public abstract class AttachmentBase : IDisposable
    {
        internal bool disposed;
        private System.Net.Mime.MimePart part;

        internal AttachmentBase()
        {
            this.part = new System.Net.Mime.MimePart();
        }

        protected AttachmentBase(Stream contentStream)
        {
            this.part = new System.Net.Mime.MimePart();
            this.part.SetContent(contentStream);
        }

        protected AttachmentBase(string fileName)
        {
            this.part = new System.Net.Mime.MimePart();
            this.SetContentFromFile(fileName, string.Empty);
        }

        protected AttachmentBase(Stream contentStream, System.Net.Mime.ContentType contentType)
        {
            this.part = new System.Net.Mime.MimePart();
            this.part.SetContent(contentStream, contentType);
        }

        protected AttachmentBase(Stream contentStream, string mediaType)
        {
            this.part = new System.Net.Mime.MimePart();
            this.part.SetContent(contentStream, null, mediaType);
        }

        protected AttachmentBase(string fileName, System.Net.Mime.ContentType contentType)
        {
            this.part = new System.Net.Mime.MimePart();
            this.SetContentFromFile(fileName, contentType);
        }

        protected AttachmentBase(string fileName, string mediaType)
        {
            this.part = new System.Net.Mime.MimePart();
            this.SetContentFromFile(fileName, mediaType);
        }

        internal AttachmentBase(Stream contentStream, string name, string mediaType)
        {
            this.part = new System.Net.Mime.MimePart();
            this.part.SetContent(contentStream, name, mediaType);
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
                this.part.Dispose();
            }
        }

        internal virtual void PrepareForSending()
        {
            this.part.ResetStream();
        }

        internal void SetContentFromFile(string fileName, System.Net.Mime.ContentType contentType)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (fileName == string.Empty)
            {
                throw new ArgumentException(SR.GetString("net_emptystringcall", new object[] { "fileName" }), "fileName");
            }
            Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            this.part.SetContent(stream, contentType);
        }

        internal void SetContentFromFile(string fileName, string mediaType)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (fileName == string.Empty)
            {
                throw new ArgumentException(SR.GetString("net_emptystringcall", new object[] { "fileName" }), "fileName");
            }
            Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            this.part.SetContent(stream, null, mediaType);
        }

        internal void SetContentFromString(string contentString, System.Net.Mime.ContentType contentType)
        {
            Encoding aSCII;
            if (contentString == null)
            {
                throw new ArgumentNullException("content");
            }
            if (this.part.Stream != null)
            {
                this.part.Stream.Close();
            }
            if ((contentType != null) && (contentType.CharSet != null))
            {
                aSCII = Encoding.GetEncoding(contentType.CharSet);
            }
            else if (MimeBasePart.IsAscii(contentString, false))
            {
                aSCII = Encoding.ASCII;
            }
            else
            {
                aSCII = Encoding.GetEncoding("utf-8");
            }
            byte[] bytes = aSCII.GetBytes(contentString);
            this.part.SetContent(new MemoryStream(bytes), contentType);
            if (MimeBasePart.ShouldUseBase64Encoding(aSCII))
            {
                this.part.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            }
            else
            {
                this.part.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;
            }
        }

        internal void SetContentFromString(string contentString, Encoding encoding, string mediaType)
        {
            if (contentString == null)
            {
                throw new ArgumentNullException("content");
            }
            if (this.part.Stream != null)
            {
                this.part.Stream.Close();
            }
            if ((mediaType == null) || (mediaType == string.Empty))
            {
                mediaType = "text/plain";
            }
            int offset = 0;
            try
            {
                if (((MailBnfHelper.ReadToken(mediaType, ref offset, null).Length == 0) || (offset >= mediaType.Length)) || (mediaType[offset++] != '/'))
                {
                    throw new ArgumentException(SR.GetString("MediaTypeInvalid"), "mediaType");
                }
                if ((MailBnfHelper.ReadToken(mediaType, ref offset, null).Length == 0) || (offset < mediaType.Length))
                {
                    throw new ArgumentException(SR.GetString("MediaTypeInvalid"), "mediaType");
                }
            }
            catch (FormatException)
            {
                throw new ArgumentException(SR.GetString("MediaTypeInvalid"), "mediaType");
            }
            System.Net.Mime.ContentType contentType = new System.Net.Mime.ContentType(mediaType);
            if (encoding == null)
            {
                if (MimeBasePart.IsAscii(contentString, false))
                {
                    encoding = Encoding.ASCII;
                }
                else
                {
                    encoding = Encoding.GetEncoding("utf-8");
                }
            }
            contentType.CharSet = encoding.BodyName;
            byte[] bytes = encoding.GetBytes(contentString);
            this.part.SetContent(new MemoryStream(bytes), contentType);
            if (MimeBasePart.ShouldUseBase64Encoding(encoding))
            {
                this.part.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            }
            else
            {
                this.part.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;
            }
        }

        internal static string ShortNameFromFile(string fileName)
        {
            int num = fileName.LastIndexOfAny(new char[] { '\\', ':' }, fileName.Length - 1, fileName.Length);
            if (num > 0)
            {
                return fileName.Substring(num + 1, (fileName.Length - num) - 1);
            }
            return fileName;
        }

        public string ContentId
        {
            get
            {
                string contentID = this.part.ContentID;
                if (string.IsNullOrEmpty(contentID))
                {
                    contentID = Guid.NewGuid().ToString();
                    this.ContentId = contentID;
                    return contentID;
                }
                if (((contentID.Length >= 2) && (contentID[0] == '<')) && (contentID[contentID.Length - 1] == '>'))
                {
                    return contentID.Substring(1, contentID.Length - 2);
                }
                return contentID;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.part.ContentID = null;
                }
                else
                {
                    if (value.IndexOfAny(new char[] { '<', '>' }) != -1)
                    {
                        throw new ArgumentException(SR.GetString("MailHeaderInvalidCID"), "value");
                    }
                    this.part.ContentID = "<" + value + ">";
                }
            }
        }

        internal Uri ContentLocation
        {
            get
            {
                Uri uri;
                if (!Uri.TryCreate(this.part.ContentLocation, UriKind.RelativeOrAbsolute, out uri))
                {
                    return null;
                }
                return uri;
            }
            set
            {
                this.part.ContentLocation = (value == null) ? null : (value.IsAbsoluteUri ? value.AbsoluteUri : value.OriginalString);
            }
        }

        public Stream ContentStream
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                return this.part.Stream;
            }
        }

        public System.Net.Mime.ContentType ContentType
        {
            get
            {
                return this.part.ContentType;
            }
            set
            {
                this.part.ContentType = value;
            }
        }

        internal System.Net.Mime.MimePart MimePart
        {
            get
            {
                return this.part;
            }
        }

        public System.Net.Mime.TransferEncoding TransferEncoding
        {
            get
            {
                return this.part.TransferEncoding;
            }
            set
            {
                this.part.TransferEncoding = value;
            }
        }
    }
}

