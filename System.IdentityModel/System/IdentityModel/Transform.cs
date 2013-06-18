namespace System.IdentityModel
{
    using System;
    using System.Xml;

    internal abstract class Transform
    {
        protected Transform()
        {
        }

        public abstract object Process(object input, SignatureResourcePool resourcePool, DictionaryManager dictionaryManager);
        public abstract byte[] ProcessAndDigest(object input, SignatureResourcePool resourcePool, string digestAlgorithm, DictionaryManager dictionaryManager);
        public abstract void ReadFrom(XmlDictionaryReader reader, DictionaryManager dictionaryManager);
        public abstract void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager);

        public abstract string Algorithm { get; }

        public virtual bool NeedsInclusiveContext
        {
            get
            {
                return false;
            }
        }
    }
}

