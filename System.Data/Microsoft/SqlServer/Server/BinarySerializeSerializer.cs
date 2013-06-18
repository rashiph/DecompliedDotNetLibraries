namespace Microsoft.SqlServer.Server
{
    using System;
    using System.IO;

    internal sealed class BinarySerializeSerializer : Serializer
    {
        internal BinarySerializeSerializer(Type t) : base(t)
        {
        }

        public override object Deserialize(Stream s)
        {
            object obj2 = Activator.CreateInstance(base.m_type);
            BinaryReader r = new BinaryReader(s);
            ((IBinarySerialize) obj2).Read(r);
            return obj2;
        }

        public override void Serialize(Stream s, object o)
        {
            BinaryWriter w = new BinaryWriter(s);
            ((IBinarySerialize) o).Write(w);
        }
    }
}

