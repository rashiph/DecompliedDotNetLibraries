namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Xml;

    internal sealed class ConfigXmlCDataSection : XmlCDataSection, IConfigErrorInfo
    {
        private string _filename;
        private int _line;

        public ConfigXmlCDataSection(string filename, int line, string data, XmlDocument doc) : base(data, doc)
        {
            this._line = line;
            this._filename = filename;
        }

        public override XmlNode CloneNode(bool deep)
        {
            XmlNode node = base.CloneNode(deep);
            System.Configuration.ConfigXmlCDataSection section = node as System.Configuration.ConfigXmlCDataSection;
            if (section != null)
            {
                section._line = this._line;
                section._filename = this._filename;
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

