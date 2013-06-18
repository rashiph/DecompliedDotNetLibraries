namespace System.ServiceModel.Channels
{
    using System;
    using System.Text;
    using System.Xml;

    internal class MtomMessageEncoderFactory : MessageEncoderFactory
    {
        private MtomMessageEncoder messageEncoder;

        public MtomMessageEncoderFactory(System.ServiceModel.Channels.MessageVersion version, Encoding writeEncoding, int maxReadPoolSize, int maxWritePoolSize, int maxBufferSize, XmlDictionaryReaderQuotas quotas)
        {
            this.messageEncoder = new MtomMessageEncoder(version, writeEncoding, maxReadPoolSize, maxWritePoolSize, maxBufferSize, quotas);
        }

        public static Encoding[] GetSupportedEncodings()
        {
            Encoding[] supportedEncodings = TextEncoderDefaults.SupportedEncodings;
            Encoding[] destinationArray = new Encoding[supportedEncodings.Length];
            Array.Copy(supportedEncodings, destinationArray, supportedEncodings.Length);
            return destinationArray;
        }

        public override MessageEncoder Encoder
        {
            get
            {
                return this.messageEncoder;
            }
        }

        public int MaxBufferSize
        {
            get
            {
                return this.messageEncoder.MaxBufferSize;
            }
        }

        public int MaxReadPoolSize
        {
            get
            {
                return this.messageEncoder.MaxReadPoolSize;
            }
        }

        public int MaxWritePoolSize
        {
            get
            {
                return this.messageEncoder.MaxWritePoolSize;
            }
        }

        public override System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageEncoder.MessageVersion;
            }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.messageEncoder.ReaderQuotas;
            }
        }
    }
}

