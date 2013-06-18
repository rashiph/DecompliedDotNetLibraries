namespace System.ServiceModel.Channels
{
    using System;

    internal class EncodedContentType : EncodedFramingRecord
    {
        private EncodedContentType(FramingEncodingType encodingType) : base(new byte[] { 3, (byte) encodingType })
        {
        }

        private EncodedContentType(string contentType) : base(FramingRecordType.ExtensibleEncoding, contentType)
        {
        }

        public static EncodedContentType Create(string contentType)
        {
            if (contentType == "application/soap+msbinsession1")
            {
                return new EncodedContentType(FramingEncodingType.BinarySession);
            }
            if (contentType == "application/soap+msbin1")
            {
                return new EncodedContentType(FramingEncodingType.Binary);
            }
            if (contentType == "application/soap+xml; charset=utf-8")
            {
                return new EncodedContentType(FramingEncodingType.Soap12Utf8);
            }
            if (contentType == "text/xml; charset=utf-8")
            {
                return new EncodedContentType(FramingEncodingType.Soap11Utf8);
            }
            if (contentType == "application/soap+xml; charset=utf16")
            {
                return new EncodedContentType(FramingEncodingType.Soap12Utf16);
            }
            if (contentType == "text/xml; charset=utf16")
            {
                return new EncodedContentType(FramingEncodingType.Soap11Utf16);
            }
            if (contentType == "application/soap+xml; charset=unicodeFFFE")
            {
                return new EncodedContentType(FramingEncodingType.Soap12Utf16FFFE);
            }
            if (contentType == "text/xml; charset=unicodeFFFE")
            {
                return new EncodedContentType(FramingEncodingType.Soap11Utf16FFFE);
            }
            if (contentType == "multipart/related")
            {
                return new EncodedContentType(FramingEncodingType.MTOM);
            }
            return new EncodedContentType(contentType);
        }
    }
}

