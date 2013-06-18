namespace System.Data
{
    using System;
    using System.Collections;
    using System.Runtime.Serialization;

    [Serializable]
    public class PropertyCollection : Hashtable
    {
        public PropertyCollection()
        {
        }

        protected PropertyCollection(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

