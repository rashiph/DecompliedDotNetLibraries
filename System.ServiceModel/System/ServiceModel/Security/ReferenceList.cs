namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class ReferenceList : ISecurityElement
    {
        internal static readonly XmlDictionaryString ElementName = System.ServiceModel.XD.XmlEncryptionDictionary.ReferenceList;
        private const string NamespacePrefix = "e";
        internal static readonly XmlDictionaryString NamespaceUri = EncryptedType.NamespaceUri;
        private System.ServiceModel.MostlySingletonList<string> referredIds;
        internal static readonly XmlDictionaryString UriAttribute = System.ServiceModel.XD.XmlEncryptionDictionary.URI;

        public void AddReferredId(string id)
        {
            if (id == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("id"));
            }
            this.referredIds.Add(id);
        }

        public bool ContainsReferredId(string id)
        {
            if (id == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("id"));
            }
            return this.referredIds.Contains(id);
        }

        public string GetReferredId(int index)
        {
            return this.referredIds[index];
        }

        public void ReadFrom(XmlDictionaryReader reader)
        {
            reader.ReadStartElement(ElementName, NamespaceUri);
            while (reader.IsStartElement())
            {
                string item = DataReference.ReadFrom(reader);
                if (this.referredIds.Contains(item))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("InvalidDataReferenceInReferenceList", new object[] { "#" + item })));
                }
                this.referredIds.Add(item);
            }
            reader.ReadEndElement();
            if (this.DataReferenceCount == 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("ReferenceListCannotBeEmpty")));
            }
        }

        public bool TryRemoveReferredId(string id)
        {
            if (id == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("id"));
            }
            return this.referredIds.Remove(id);
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            if (this.DataReferenceCount == 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ReferenceListCannotBeEmpty")));
            }
            writer.WriteStartElement("e", ElementName, NamespaceUri);
            for (int i = 0; i < this.DataReferenceCount; i++)
            {
                DataReference.WriteTo(writer, this.referredIds[i]);
            }
            writer.WriteEndElement();
        }

        public int DataReferenceCount
        {
            get
            {
                return this.referredIds.Count;
            }
        }

        public bool HasId
        {
            get
            {
                return false;
            }
        }

        public string Id
        {
            get
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }

        private static class DataReference
        {
            internal static readonly XmlDictionaryString ElementName = System.ServiceModel.XD.XmlEncryptionDictionary.DataReference;
            internal static readonly XmlDictionaryString NamespaceUri = EncryptedType.NamespaceUri;

            public static string ReadFrom(XmlDictionaryReader reader)
            {
                string str;
                string str2 = System.ServiceModel.Security.XmlHelper.ReadEmptyElementAndRequiredAttribute(reader, ElementName, NamespaceUri, ReferenceList.UriAttribute, out str);
                if ((str2.Length < 2) || (str2[0] != '#'))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("InvalidDataReferenceInReferenceList", new object[] { str2 })));
                }
                return str2.Substring(1);
            }

            public static void WriteTo(XmlDictionaryWriter writer, string referredId)
            {
                writer.WriteStartElement(System.ServiceModel.XD.XmlEncryptionDictionary.Prefix.Value, ElementName, NamespaceUri);
                writer.WriteStartAttribute(ReferenceList.UriAttribute, null);
                writer.WriteString("#");
                writer.WriteString(referredId);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
        }
    }
}

