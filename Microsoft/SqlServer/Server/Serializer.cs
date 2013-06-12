namespace Microsoft.SqlServer.Server
{
    using System;
    using System.IO;

    internal abstract class Serializer
    {
        protected Type m_type;

        protected Serializer(Type t)
        {
            this.m_type = t;
        }

        public abstract object Deserialize(Stream s);
        public abstract void Serialize(Stream s, object o);
    }
}

