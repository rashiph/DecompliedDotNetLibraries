namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Xml;

    internal sealed class ConfigXmlText : XmlText, IConfigErrorInfo
    {
        private string _filename;
        private int _line;

        public ConfigXmlText(string filename, int line, string strData, XmlDocument doc) : base(strData, doc)
        {
            this._line = line;
            this._filename = filename;
        }

        public override XmlNode CloneNode(bool deep)
        {
            XmlNode node = base.CloneNode(deep);
            System.Configuration.ConfigXmlText text = node as System.Configuration.ConfigXmlText;
            if (text != null)
            {
                text._line = this._line;
                text._filename = this._filename;
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

