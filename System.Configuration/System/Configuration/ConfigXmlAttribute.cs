namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Xml;

    internal sealed class ConfigXmlAttribute : XmlAttribute, IConfigErrorInfo
    {
        private string _filename;
        private int _line;

        public ConfigXmlAttribute(string filename, int line, string prefix, string localName, string namespaceUri, XmlDocument doc) : base(prefix, localName, namespaceUri, doc)
        {
            this._line = line;
            this._filename = filename;
        }

        public override XmlNode CloneNode(bool deep)
        {
            XmlNode node = base.CloneNode(deep);
            System.Configuration.ConfigXmlAttribute attribute = node as System.Configuration.ConfigXmlAttribute;
            if (attribute != null)
            {
                attribute._line = this._line;
                attribute._filename = this._filename;
            }
            return node;
        }

        string IConfigErrorInfo.Filename
        {
            get
            {
                return this._filename;
            }
        }

        int IConfigErrorInfo.LineNumber
        {
            get
            {
                return this._line;
            }
        }
    }
}

