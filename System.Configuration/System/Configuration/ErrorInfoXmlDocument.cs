namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Xml;

    internal sealed class ErrorInfoXmlDocument : XmlDocument, IConfigErrorInfo
    {
        private string _filename;
        private int _lineOffset;
        private XmlTextReader _reader;

        public override XmlAttribute CreateAttribute(string prefix, string localName, string namespaceUri)
        {
            return new System.Configuration.ConfigXmlAttribute(this._filename, this.LineNumber, prefix, localName, namespaceUri, this);
        }

        public override XmlCDataSection CreateCDataSection(string data)
        {
            return new System.Configuration.ConfigXmlCDataSection(this._filename, this.LineNumber, data, this);
        }

        public override XmlComment CreateComment(string data)
        {
            return new System.Configuration.ConfigXmlComment(this._filename, this.LineNumber, data, this);
        }

        public override XmlElement CreateElement(string prefix, string localName, string namespaceUri)
        {
            return new System.Configuration.ConfigXmlElement(this._filename, this.LineNumber, prefix, localName, namespaceUri, this);
        }

        internal static XmlNode CreateSectionXmlNode(ConfigXmlReader reader)
        {
            ErrorInfoXmlDocument document = new ErrorInfoXmlDocument();
            document.LoadFromConfigXmlReader(reader);
            return document.DocumentElement;
        }

        public override XmlSignificantWhitespace CreateSignificantWhitespace(string data)
        {
            return new System.Configuration.ConfigXmlSignificantWhitespace(this._filename, this.LineNumber, data, this);
        }

        public override XmlText CreateTextNode(string text)
        {
            return new System.Configuration.ConfigXmlText(this._filename, this.LineNumber, text, this);
        }

        public override XmlWhitespace CreateWhitespace(string data)
        {
            return new System.Configuration.ConfigXmlWhitespace(this._filename, this.LineNumber, data, this);
        }

        public override void Load(string filename)
        {
            this._filename = filename;
            try
            {
                this._reader = new XmlTextReader(filename);
                this._reader.XmlResolver = null;
                base.Load(this._reader);
            }
            finally
            {
                if (this._reader != null)
                {
                    this._reader.Close();
                    this._reader = null;
                }
            }
        }

        private void LoadFromConfigXmlReader(ConfigXmlReader reader)
        {
            IConfigErrorInfo info = reader;
            this._filename = info.Filename;
            this._lineOffset = info.LineNumber + 1;
            try
            {
                this._reader = reader;
                base.Load(this._reader);
            }
            finally
            {
                if (this._reader != null)
                {
                    this._reader.Close();
                    this._reader = null;
                }
            }
        }

        internal int LineNumber
        {
            get
            {
                return ((IConfigErrorInfo) this).LineNumber;
            }
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
                if (this._reader == null)
                {
                    return 0;
                }
                if (this._lineOffset > 0)
                {
                    return ((this._reader.LineNumber + this._lineOffset) - 1);
                }
                return this._reader.LineNumber;
            }
        }
    }
}

