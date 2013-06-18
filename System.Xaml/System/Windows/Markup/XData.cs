namespace System.Windows.Markup
{
    using System;
    using System.IO;
    using System.Xml;

    [ContentProperty("Text")]
    public sealed class XData
    {
        private System.Xml.XmlReader _reader;
        private string _text;

        public string Text
        {
            get
            {
                return this._text;
            }
            set
            {
                this._text = value;
                this._reader = null;
            }
        }

        public object XmlReader
        {
            get
            {
                if (this._reader == null)
                {
                    StringReader input = new StringReader(this.Text);
                    this._reader = System.Xml.XmlReader.Create(input);
                }
                return this._reader;
            }
            set
            {
                this._reader = value as System.Xml.XmlReader;
                this._text = null;
            }
        }
    }
}

