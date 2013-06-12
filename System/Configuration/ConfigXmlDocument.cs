namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.IO;
    using System.Security.Permissions;
    using System.Xml;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class ConfigXmlDocument : XmlDocument, IConfigErrorInfo
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

        public void LoadSingleElement(string filename, XmlTextReader sourceReader)
        {
            this._filename = filename;
            this._lineOffset = sourceReader.LineNumber;
            string s = sourceReader.ReadOuterXml();
            try
            {
                this._reader = new XmlTextReader(new StringReader(s), sourceReader.NameTable);
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

        public string Filename
        {
            get
            {
                return ConfigurationException.SafeFilename(this._filename);
            }
        }

        public int LineNumber
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

