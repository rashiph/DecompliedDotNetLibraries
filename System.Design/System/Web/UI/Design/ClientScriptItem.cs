namespace System.Web.UI.Design
{
    using System;

    public sealed class ClientScriptItem
    {
        private string _id;
        private string _language;
        private string _source;
        private string _text;
        private string _type;

        public ClientScriptItem(string text, string source, string language, string type, string id)
        {
            this._text = text;
            this._source = source;
            this._language = language;
            this._type = type;
            this._id = id;
        }

        public string Id
        {
            get
            {
                return this._id;
            }
        }

        public string Language
        {
            get
            {
                return this._language;
            }
        }

        public string Source
        {
            get
            {
                return this._source;
            }
        }

        public string Text
        {
            get
            {
                return this._text;
            }
        }

        public string Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

