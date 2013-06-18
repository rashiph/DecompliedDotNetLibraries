namespace System.IdentityModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Xml;

    internal sealed class StandardSignedInfo : SignedInfo
    {
        private Dictionary<string, string> context;
        private string prefix;
        private List<Reference> references;

        public StandardSignedInfo(DictionaryManager dictionaryManager) : base(dictionaryManager)
        {
            this.prefix = "";
            this.references = new List<Reference>();
        }

        public void AddReference(Reference reference)
        {
            reference.ResourcePool = base.ResourcePool;
            this.references.Add(reference);
        }

        public override void ComputeReferenceDigests()
        {
            if (this.references.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("AtLeastOneReferenceRequired")));
            }
            for (int i = 0; i < this.references.Count; i++)
            {
                this.references[i].ComputeAndSetDigest();
            }
        }

        public override void EnsureAllReferencesVerified()
        {
            for (int i = 0; i < this.references.Count; i++)
            {
                if (!this.references[i].Verified)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnableToResolveReferenceUriForSignature", new object[] { this.references[i].Uri })));
                }
            }
        }

        public override bool EnsureDigestValidityIfIdMatches(string id, object resolvedXmlSource)
        {
            for (int i = 0; i < this.references.Count; i++)
            {
                if (this.references[i].EnsureDigestValidityIfIdMatches(id, resolvedXmlSource))
                {
                    return true;
                }
            }
            return false;
        }

        protected override string GetNamespaceForInclusivePrefix(string prefix)
        {
            if (this.context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            }
            if (prefix == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");
            }
            return this.context[prefix];
        }

        public override bool HasUnverifiedReference(string id)
        {
            for (int i = 0; i < this.references.Count; i++)
            {
                if (!this.references[i].Verified && (this.references[i].ExtractReferredId() == id))
                {
                    return true;
                }
            }
            return false;
        }

        public override void ReadFrom(XmlDictionaryReader reader, TransformFactory transformFactory, DictionaryManager dictionaryManager)
        {
            base.SendSide = false;
            if (reader.CanCanonicalize)
            {
                base.CanonicalStream = new MemoryStream();
                reader.StartCanonicalization(base.CanonicalStream, false, null);
            }
            reader.MoveToStartElement(dictionaryManager.XmlSignatureDictionary.SignedInfo, dictionaryManager.XmlSignatureDictionary.Namespace);
            this.prefix = reader.Prefix;
            base.Id = reader.GetAttribute(dictionaryManager.UtilityDictionary.IdAttribute, null);
            reader.Read();
            base.ReadCanonicalizationMethod(reader, dictionaryManager);
            base.ReadSignatureMethod(reader, dictionaryManager);
            while (reader.IsStartElement(dictionaryManager.XmlSignatureDictionary.Reference, dictionaryManager.XmlSignatureDictionary.Namespace))
            {
                Reference reference = new Reference(dictionaryManager);
                reference.ReadFrom(reader, transformFactory, dictionaryManager);
                this.AddReference(reference);
            }
            reader.ReadEndElement();
            if (reader.CanCanonicalize)
            {
                reader.EndCanonicalization();
            }
            string[] inclusivePrefixes = base.GetInclusivePrefixes();
            if (inclusivePrefixes != null)
            {
                base.CanonicalStream = null;
                this.context = new Dictionary<string, string>(inclusivePrefixes.Length);
                for (int i = 0; i < inclusivePrefixes.Length; i++)
                {
                    this.context.Add(inclusivePrefixes[i], reader.LookupNamespace(inclusivePrefixes[i]));
                }
            }
        }

        public override void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            writer.WriteStartElement(this.prefix, dictionaryManager.XmlSignatureDictionary.SignedInfo, dictionaryManager.XmlSignatureDictionary.Namespace);
            if (base.Id != null)
            {
                writer.WriteAttributeString(dictionaryManager.UtilityDictionary.IdAttribute, null, base.Id);
            }
            base.WriteCanonicalizationMethod(writer, dictionaryManager);
            base.WriteSignatureMethod(writer, dictionaryManager);
            for (int i = 0; i < this.references.Count; i++)
            {
                this.references[i].WriteTo(writer, dictionaryManager);
            }
            writer.WriteEndElement();
        }

        public Reference this[int index]
        {
            get
            {
                return this.references[index];
            }
        }

        public override int ReferenceCount
        {
            get
            {
                return this.references.Count;
            }
        }
    }
}

