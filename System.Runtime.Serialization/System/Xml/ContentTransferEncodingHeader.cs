namespace System.Xml
{
    using System;

    internal class ContentTransferEncodingHeader : MimeHeader
    {
        public static readonly ContentTransferEncodingHeader Binary = new ContentTransferEncodingHeader(System.Xml.ContentTransferEncoding.Binary, "binary");
        private System.Xml.ContentTransferEncoding contentTransferEncoding;
        private string contentTransferEncodingValue;
        public static readonly ContentTransferEncodingHeader EightBit = new ContentTransferEncodingHeader(System.Xml.ContentTransferEncoding.EightBit, "8bit");
        public static readonly ContentTransferEncodingHeader SevenBit = new ContentTransferEncodingHeader(System.Xml.ContentTransferEncoding.SevenBit, "7bit");

        public ContentTransferEncodingHeader(string value) : base("content-transfer-encoding", value.ToLowerInvariant())
        {
        }

        public ContentTransferEncodingHeader(System.Xml.ContentTransferEncoding contentTransferEncoding, string value) : base("content-transfer-encoding", null)
        {
            this.contentTransferEncoding = contentTransferEncoding;
            this.contentTransferEncodingValue = value;
        }

        private void ParseValue()
        {
            if (this.contentTransferEncodingValue == null)
            {
                int offset = 0;
                this.contentTransferEncodingValue = (base.Value.Length == 0) ? base.Value : ((base.Value[0] == '"') ? MailBnfHelper.ReadQuotedString(base.Value, ref offset, null) : MailBnfHelper.ReadToken(base.Value, ref offset, null));
                switch (this.contentTransferEncodingValue)
                {
                    case "7bit":
                        this.contentTransferEncoding = System.Xml.ContentTransferEncoding.SevenBit;
                        return;

                    case "8bit":
                        this.contentTransferEncoding = System.Xml.ContentTransferEncoding.EightBit;
                        return;

                    case "binary":
                        this.contentTransferEncoding = System.Xml.ContentTransferEncoding.Binary;
                        return;
                }
                this.contentTransferEncoding = System.Xml.ContentTransferEncoding.Other;
            }
        }

        public System.Xml.ContentTransferEncoding ContentTransferEncoding
        {
            get
            {
                this.ParseValue();
                return this.contentTransferEncoding;
            }
        }

        public string ContentTransferEncodingValue
        {
            get
            {
                this.ParseValue();
                return this.contentTransferEncodingValue;
            }
        }
    }
}

