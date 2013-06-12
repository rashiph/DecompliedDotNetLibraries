namespace System.Text
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal sealed class CodePageEncoding : ISerializable, IObjectReference
    {
        [NonSerialized]
        private DecoderFallback decoderFallback;
        [NonSerialized]
        private EncoderFallback encoderFallback;
        [NonSerialized]
        private int m_codePage;
        [NonSerialized]
        private bool m_deserializedFromEverett;
        [NonSerialized]
        private bool m_isReadOnly;
        [NonSerialized]
        private Encoding realEncoding;

        internal CodePageEncoding(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.m_codePage = (int) info.GetValue("m_codePage", typeof(int));
            try
            {
                this.m_isReadOnly = (bool) info.GetValue("m_isReadOnly", typeof(bool));
                this.encoderFallback = (EncoderFallback) info.GetValue("encoderFallback", typeof(EncoderFallback));
                this.decoderFallback = (DecoderFallback) info.GetValue("decoderFallback", typeof(DecoderFallback));
            }
            catch (SerializationException)
            {
                this.m_deserializedFromEverett = true;
                this.m_isReadOnly = true;
            }
        }

        [SecurityCritical]
        public object GetRealObject(StreamingContext context)
        {
            this.realEncoding = Encoding.GetEncoding(this.m_codePage);
            if (!this.m_deserializedFromEverett && !this.m_isReadOnly)
            {
                this.realEncoding = (Encoding) this.realEncoding.Clone();
                this.realEncoding.EncoderFallback = this.encoderFallback;
                this.realEncoding.DecoderFallback = this.decoderFallback;
            }
            return this.realEncoding;
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_ExecutionEngineException"));
        }

        [Serializable]
        internal sealed class Decoder : ISerializable, IObjectReference
        {
            [NonSerialized]
            private Encoding realEncoding;

            internal Decoder(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                this.realEncoding = (Encoding) info.GetValue("encoding", typeof(Encoding));
            }

            [SecurityCritical]
            public object GetRealObject(StreamingContext context)
            {
                return this.realEncoding.GetDecoder();
            }

            [SecurityCritical]
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_ExecutionEngineException"));
            }
        }
    }
}

