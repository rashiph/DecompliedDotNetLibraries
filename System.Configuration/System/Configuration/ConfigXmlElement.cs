namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Xml;

    internal sealed class ConfigXmlElement : XmlElement, IConfigErrorInfo
    {
        private string _filename;
        private int _line;

        public ConfigXmlElement(string filename, int line, string prefix, string localName, string namespaceUri, XmlDocument doc) : base(prefix, localName, namespaceUri, doc)
        {
            this._line = line;
            this._filename = filename;
        }

        public override XmlNode CloneNode(bool deep)
        {
            XmlNode node = base.CloneNode(deep);
            System.Configuration.ConfigXmlElement element = node as System.Configuration.ConfigXmlElement;
            if (element != null)
            {
                element._line = this._line;
                element._filename = this._filename;
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

