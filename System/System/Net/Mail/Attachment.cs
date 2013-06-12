namespace System.Net.Mail
{
    using System;
    using System.IO;
    using System.Net.Mime;
    using System.Text;

    public class Attachment : AttachmentBase
    {
        private string name;
        private Encoding nameEncoding;

        internal Attachment()
        {
            base.MimePart.ContentDisposition = new System.Net.Mime.ContentDisposition();
        }

        public Attachment(string fileName) : base(fileName)
        {
            this.Name = AttachmentBase.ShortNameFromFile(fileName);
            base.MimePart.ContentDisposition = new System.Net.Mime.ContentDisposition();
        }

        public Attachment(Stream contentStream, ContentType contentType) : base(contentStream, contentType)
        {
            this.Name = contentType.Name;
            base.MimePart.ContentDisposition = new System.Net.Mime.ContentDisposition();
        }

        public Attachment(Stream contentStream, string name) : base(contentStream, null, null)
        {
            this.Name = name;
            base.MimePart.ContentDisposition = new System.Net.Mime.ContentDisposition();
        }

        public Attachment(string fileName, ContentType contentType) : base(fileName, contentType)
        {
            if ((contentType.Name == null) || (contentType.Name == string.Empty))
            {
                this.Name = AttachmentBase.ShortNameFromFile(fileName);
            }
            else
            {
                this.Name = contentType.Name;
            }
            base.MimePart.ContentDisposition = new System.Net.Mime.ContentDisposition();
        }

        public Attachment(string fileName, string mediaType) : base(fileName, mediaType)
        {
            this.Name = AttachmentBase.ShortNameFromFile(fileName);
            base.MimePart.ContentDisposition = new System.Net.Mime.ContentDisposition();
        }

        public Attachment(Stream contentStream, string name, string mediaType) : base(contentStream, null, mediaType)
        {
            this.Name = name;
            base.MimePart.ContentDisposition = new System.Net.Mime.ContentDisposition();
        }

        public static Attachment CreateAttachmentFromString(string content, ContentType contentType)
        {
            Attachment attachment = new Attachment();
            attachment.SetContentFromString(content, contentType);
            attachment.Name = contentType.Name;
            return attachment;
        }

        public static Attachment CreateAttachmentFromString(string content, string name)
        {
            Attachment attachment = new Attachment();
            attachment.SetContentFromString(content, null, string.Empty);
            attachment.Name = name;
            return attachment;
        }

        public static Attachment CreateAttachmentFromString(string content, string name, Encoding contentEncoding, string mediaType)
        {
            Attachment attachment = new Attachment();
            attachment.SetContentFromString(content, contentEncoding, mediaType);
            attachment.Name = name;
            return attachment;
        }

        internal override void PrepareForSending()
        {
            if ((this.name != null) && (this.name != string.Empty))
            {
                this.SetContentTypeName();
            }
            base.PrepareForSending();
        }

        internal void SetContentTypeName()
        {
            if (((this.name != null) && (this.name.Length != 0)) && !MimeBasePart.IsAscii(this.name, false))
            {
                Encoding nameEncoding = this.NameEncoding;
                if (nameEncoding == null)
                {
                    nameEncoding = Encoding.GetEncoding("utf-8");
                }
                base.MimePart.ContentType.Name = MimeBasePart.EncodeHeaderValue(this.name, nameEncoding, MimeBasePart.ShouldUseBase64Encoding(nameEncoding));
            }
            else
            {
                base.MimePart.ContentType.Name = this.name;
            }
        }

        public System.Net.Mime.ContentDisposition ContentDisposition
        {
            get
            {
                return base.MimePart.ContentDisposition;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                Encoding encoding = MimeBasePart.DecodeEncoding(value);
                if (encoding != null)
                {
                    this.nameEncoding = encoding;
                    this.name = MimeBasePart.DecodeHeaderValue(value);
                    base.MimePart.ContentType.Name = value;
                }
                else
                {
                    this.name = value;
                    this.SetContentTypeName();
                }
            }
        }

        public Encoding NameEncoding
        {
            get
            {
                return this.nameEncoding;
            }
            set
            {
                this.nameEncoding = value;
                if ((this.name != null) && (this.name != string.Empty))
                {
                    this.SetContentTypeName();
                }
            }
        }
    }
}

