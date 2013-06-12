namespace System.Xml.Serialization
{
    using System;
    using System.Collections;

    public abstract class XmlSerializerImplementation
    {
        protected XmlSerializerImplementation()
        {
        }

        public virtual bool CanSerialize(Type type)
        {
            throw new NotSupportedException();
        }

        public virtual XmlSerializer GetSerializer(Type type)
        {
            throw new NotSupportedException();
        }

        public virtual XmlSerializationReader Reader
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public virtual Hashtable ReadMethods
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public virtual Hashtable TypedSerializers
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public virtual Hashtable WriteMethods
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public virtual XmlSerializationWriter Writer
        {
            get
            {
                throw new NotSupportedException();
            }
        }
    }
}

