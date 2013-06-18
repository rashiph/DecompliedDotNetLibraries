namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ElementWithAlgorithmAttribute
    {
        private readonly XmlDictionaryString elementName;
        private string algorithm;
        private XmlDictionaryString algorithmDictionaryString;
        private string prefix;
        public ElementWithAlgorithmAttribute(XmlDictionaryString elementName)
        {
            if (elementName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("elementName"));
            }
            this.elementName = elementName;
            this.algorithm = null;
            this.algorithmDictionaryString = null;
            this.prefix = "";
        }

        public string Algorithm
        {
            get
            {
                return this.algorithm;
            }
            set
            {
                this.algorithm = value;
            }
        }
        public XmlDictionaryString AlgorithmDictionaryString
        {
            get
            {
                return this.algorithmDictionaryString;
            }
            set
            {
                this.algorithmDictionaryString = value;
            }
        }
        public void ReadFrom(XmlDictionaryReader reader, DictionaryManager dictionaryManager)
        {
            reader.MoveToStartElement(this.elementName, dictionaryManager.XmlSignatureDictionary.Namespace);
            this.prefix = reader.Prefix;
            bool isEmptyElement = reader.IsEmptyElement;
            this.algorithm = reader.GetAttribute(dictionaryManager.XmlSignatureDictionary.Algorithm, null);
            if (this.algorithm == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("RequiredAttributeMissing", new object[] { dictionaryManager.XmlSignatureDictionary.Algorithm, this.elementName })));
            }
            reader.Read();
            reader.MoveToContent();
            if (!isEmptyElement)
            {
                reader.MoveToContent();
                reader.ReadEndElement();
            }
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            writer.WriteStartElement(this.prefix, this.elementName, dictionaryManager.XmlSignatureDictionary.Namespace);
            writer.WriteStartAttribute(dictionaryManager.XmlSignatureDictionary.Algorithm, null);
            if (this.algorithmDictionaryString != null)
            {
                writer.WriteString(this.algorithmDictionaryString);
            }
            else
            {
                writer.WriteString(this.algorithm);
            }
            writer.WriteEndAttribute();
            writer.WriteEndElement();
        }
    }
}

