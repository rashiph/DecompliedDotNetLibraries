namespace System.IdentityModel
{
    using System;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Xml;

    internal sealed class TransformChain
    {
        private string prefix = "";
        private MostlySingletonList<Transform> transforms;

        public void Add(Transform transform)
        {
            this.transforms.Add(transform);
        }

        public void ReadFrom(XmlDictionaryReader reader, TransformFactory transformFactory, DictionaryManager dictionaryManager)
        {
            reader.MoveToStartElement(dictionaryManager.XmlSignatureDictionary.Transforms, dictionaryManager.XmlSignatureDictionary.Namespace);
            this.prefix = reader.Prefix;
            reader.Read();
            while (reader.IsStartElement(dictionaryManager.XmlSignatureDictionary.Transform, dictionaryManager.XmlSignatureDictionary.Namespace))
            {
                string attribute = reader.GetAttribute(dictionaryManager.XmlSignatureDictionary.Algorithm, null);
                Transform transform = transformFactory.CreateTransform(attribute);
                transform.ReadFrom(reader, dictionaryManager);
                this.Add(transform);
            }
            reader.MoveToContent();
            reader.ReadEndElement();
            if (this.TransformCount == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("AtLeastOneTransformRequired")));
            }
        }

        public byte[] TransformToDigest(object data, SignatureResourcePool resourcePool, string digestMethod, DictionaryManager dictionaryManager)
        {
            for (int i = 0; i < (this.TransformCount - 1); i++)
            {
                data = this[i].Process(data, resourcePool, dictionaryManager);
            }
            return this[this.TransformCount - 1].ProcessAndDigest(data, resourcePool, digestMethod, dictionaryManager);
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            writer.WriteStartElement(this.prefix, dictionaryManager.XmlSignatureDictionary.Transforms, dictionaryManager.XmlSignatureDictionary.Namespace);
            for (int i = 0; i < this.TransformCount; i++)
            {
                this[i].WriteTo(writer, dictionaryManager);
            }
            writer.WriteEndElement();
        }

        public Transform this[int index]
        {
            get
            {
                return this.transforms[index];
            }
        }

        public bool NeedsInclusiveContext
        {
            get
            {
                for (int i = 0; i < this.TransformCount; i++)
                {
                    if (this[i].NeedsInclusiveContext)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public int TransformCount
        {
            get
            {
                return this.transforms.Count;
            }
        }
    }
}

