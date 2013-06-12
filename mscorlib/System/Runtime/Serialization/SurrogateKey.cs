namespace System.Runtime.Serialization
{
    using System;

    [Serializable]
    internal class SurrogateKey
    {
        internal StreamingContext m_context;
        internal Type m_type;

        internal SurrogateKey(Type type, StreamingContext context)
        {
            this.m_type = type;
            this.m_context = context;
        }

        public override int GetHashCode()
        {
            return this.m_type.GetHashCode();
        }
    }
}

