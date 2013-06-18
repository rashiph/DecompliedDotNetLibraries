namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal static class EncoderDefaults
    {
        internal const int MaxArrayLength = 0x4000;
        internal const int MaxBytesPerRead = 0x1000;
        internal const int MaxDepth = 0x20;
        internal const int MaxNameTableCharCount = 0x4000;
        internal const int MaxReadPoolSize = 0x40;
        internal const int MaxStringContentLength = 0x2000;
        internal const int MaxWritePoolSize = 0x10;
        internal static readonly XmlDictionaryReaderQuotas ReaderQuotas = GetDefaultReaderQuotas();

        private static XmlDictionaryReaderQuotas GetDefaultReaderQuotas()
        {
            return new XmlDictionaryReaderQuotas { MaxDepth = 0x20, MaxStringContentLength = 0x2000, MaxArrayLength = 0x4000, MaxBytesPerRead = 0x1000, MaxNameTableCharCount = 0x4000 };
        }

        internal static bool IsDefaultReaderQuotas(XmlDictionaryReaderQuotas quotas)
        {
            return (((quotas.MaxArrayLength == 0x4000) && (quotas.MaxBytesPerRead == 0x1000)) && (((quotas.MaxDepth == 0x20) && (quotas.MaxNameTableCharCount == 0x4000)) && (quotas.MaxStringContentLength == 0x2000)));
        }
    }
}

