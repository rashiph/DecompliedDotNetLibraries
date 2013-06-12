namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;

    internal class SurrogateHashtable : Hashtable
    {
        internal SurrogateHashtable(int size) : base(size)
        {
        }

        protected override bool KeyEquals(object key, object item)
        {
            SurrogateKey key2 = (SurrogateKey) item;
            SurrogateKey key3 = (SurrogateKey) key;
            return (((key3.m_type == key2.m_type) && ((key3.m_context.m_state & key2.m_context.m_state) == key2.m_context.m_state)) && (key3.m_context.Context == key2.m_context.Context));
        }
    }
}

