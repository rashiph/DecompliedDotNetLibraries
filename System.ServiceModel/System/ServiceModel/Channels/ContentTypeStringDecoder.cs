namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.ServiceModel;

    internal class ContentTypeStringDecoder : StringDecoder
    {
        public ContentTypeStringDecoder(int sizeQuota) : base(sizeQuota)
        {
        }

        public static string GetString(FramingEncodingType type)
        {
            switch (type)
            {
                case FramingEncodingType.Soap11Utf8:
                    return "text/xml; charset=utf-8";

                case FramingEncodingType.Soap11Utf16:
                    return "text/xml; charset=utf16";

                case FramingEncodingType.Soap11Utf16FFFE:
                    return "text/xml; charset=unicodeFFFE";

                case FramingEncodingType.Soap12Utf8:
                    return "application/soap+xml; charset=utf-8";

                case FramingEncodingType.Soap12Utf16:
                    return "application/soap+xml; charset=utf16";

                case FramingEncodingType.Soap12Utf16FFFE:
                    return "application/soap+xml; charset=unicodeFFFE";

                case FramingEncodingType.MTOM:
                    return "multipart/related";

                case FramingEncodingType.Binary:
                    return "application/soap+msbin1";

                case FramingEncodingType.BinarySession:
                    return "application/soap+msbinsession1";
            }
            int num = (int) type;
            return ("unknown" + num.ToString(CultureInfo.InvariantCulture));
        }

        protected override Exception OnSizeQuotaExceeded(int size)
        {
            Exception exception = new InvalidDataException(System.ServiceModel.SR.GetString("FramingContentTypeTooLong", new object[] { size }));
            FramingEncodingString.AddFaultString(exception, "http://schemas.microsoft.com/ws/2006/05/framing/faults/ContentTypeTooLong");
            return exception;
        }
    }
}

