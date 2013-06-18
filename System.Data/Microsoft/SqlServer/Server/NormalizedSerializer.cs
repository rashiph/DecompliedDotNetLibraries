namespace Microsoft.SqlServer.Server
{
    using System;
    using System.IO;

    internal sealed class NormalizedSerializer : Serializer
    {
        private bool m_isFixedSize;
        private int m_maxSize;
        private BinaryOrderedUdtNormalizer m_normalizer;

        internal NormalizedSerializer(Type t) : base(t)
        {
            SqlUserDefinedTypeAttribute udtAttribute = SerializationHelperSql9.GetUdtAttribute(t);
            this.m_normalizer = new BinaryOrderedUdtNormalizer(t, true);
            this.m_isFixedSize = udtAttribute.IsFixedLength;
            this.m_maxSize = this.m_normalizer.Size;
        }

        public override object Deserialize(Stream s)
        {
            return this.m_normalizer.DeNormalizeTopObject(base.m_type, s);
        }

        public override void Serialize(Stream s, object o)
        {
            this.m_normalizer.NormalizeTopObject(o, s);
        }
    }
}

