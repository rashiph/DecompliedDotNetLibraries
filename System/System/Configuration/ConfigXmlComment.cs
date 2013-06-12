namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Xml;

    internal sealed class ConfigXmlComment : XmlComment, IConfigErrorInfo
    {
        private string _filename;
        private int _line;

        public ConfigXmlComment(string filename, int line, string comment, XmlDocument doc) : base(comment, doc)
        {
            this._line = line;
            this._filename = filename;
        }

        public override XmlNode CloneNode(bool deep)
        {
            XmlNode node = base.CloneNode(deep);
            System.Configuration.ConfigXmlComment comment = node as System.Configuration.ConfigXmlComment;
            if (comment != null)
            {
                comment._line = this._line;
                comment._filename = this._filename;
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

