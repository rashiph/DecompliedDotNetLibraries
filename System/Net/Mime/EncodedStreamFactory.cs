namespace System.Net.Mime
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    internal class EncodedStreamFactory
    {
        private const int defaultMaxLineLength = 70;
        private const int initialBufferSize = 0x400;

        protected byte[] CreateFooter()
        {
            return new byte[] { 0x3f, 0x3d };
        }

        protected byte[] CreateHeader(Encoding encoding, bool useBase64Encoding)
        {
            string s = string.Format("=?{0}?{1}?", encoding.HeaderName, useBase64Encoding ? "B" : "Q");
            return Encoding.ASCII.GetBytes(s);
        }

        internal IEncodableStream GetEncoder(TransferEncoding encoding, Stream stream)
        {
            if (encoding == TransferEncoding.Base64)
            {
                return new Base64Stream(stream, new Base64WriteStateInfo(0x400, new byte[0], new byte[0], DefaultMaxLineLength));
            }
            if (encoding == TransferEncoding.QuotedPrintable)
            {
                return new QuotedPrintableStream(stream, true);
            }
            if (encoding != TransferEncoding.SevenBit)
            {
                throw new NotSupportedException("Encoding Stream");
            }
            return new SevenBitStream(stream);
        }

        internal IEncodableStream GetEncoderForHeader(Encoding encoding, bool useBase64Encoding, int headerTextLength)
        {
            WriteStateInfoBase base2;
            byte[] header = this.CreateHeader(encoding, useBase64Encoding);
            byte[] footer = this.CreateFooter();
            if (useBase64Encoding)
            {
                base2 = new Base64WriteStateInfo(0x400, header, footer, DefaultMaxLineLength) {
                    MimeHeaderLength = headerTextLength
                };
                return new Base64Stream((Base64WriteStateInfo) base2);
            }
            base2 = new QuotedStringWriteStateInfo(0x400, header, footer, DefaultMaxLineLength) {
                MimeHeaderLength = headerTextLength
            };
            return new QEncodedStream((QuotedStringWriteStateInfo) base2);
        }

        internal static int DefaultMaxLineLength
        {
            get
            {
                return 70;
            }
        }
    }
}

