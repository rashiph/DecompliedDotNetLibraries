namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public abstract class SerializationBinder
    {
        protected SerializationBinder()
        {
        }

        public virtual void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = null;
        }

        public abstract Type BindToType(string assemblyName, string typeName);
    }
}

