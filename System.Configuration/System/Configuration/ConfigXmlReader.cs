namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Xml;

    internal sealed class ConfigXmlReader : XmlTextReader, IConfigErrorInfo
    {
        private string _filename;
        private bool _lineNumberIsConstant;
        private int _lineOffset;
        private string _rawXml;

        internal ConfigXmlReader(string rawXml, string filename, int lineOffset) : this(rawXml, filename, lineOffset, false)
        {
        }

        internal ConfigXmlReader(string rawXml, string filename, int lineOffset, bool lineNumberIsConstant) : base(new StringReader(rawXml))
        {
            this._rawXml = rawXml;
            this._filename = filename;
            this._lineOffset = lineOffset;
            this._lineNumberIsConstant = lineNumberIsConstant;
        }

        internal ConfigXmlReader Clone()
        {
            return new ConfigXmlReader(this._rawXml, this._filename, this._lineOffset, this._lineNumberIsConstant);
        }

        internal string RawXml
        {
            get
            {
                return this._rawXml;
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
                if (this._lineNumberIsConstant)
                {
                    return this._lineOffset;
                }
                if (this._lineOffset > 0)
                {
                    return (base.LineNumber + (this._lineOffset - 1));
                }
                return base.LineNumber;
            }
        }
    }
}

