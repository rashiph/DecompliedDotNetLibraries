namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Xml;

    internal sealed class ConfigXmlSignificantWhitespace : XmlSignificantWhitespace, IConfigErrorInfo
    {
        private string _filename;
        private int _line;

        public ConfigXmlSignificantWhitespace(string filename, int line, string strData, XmlDocument doc) : base(strData, doc)
        {
            this._line = line;
            this._filename = filename;
        }

        public override XmlNode CloneNode(bool deep)
        {
            XmlNode node = base.CloneNode(deep);
            System.Configuration.ConfigXmlSignificantWhitespace whitespace = node as System.Configuration.ConfigXmlSignificantWhitespace;
            if (whitespace != null)
            {
                whitespace._line = this._line;
                whitespace._filename = this._filename;
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

