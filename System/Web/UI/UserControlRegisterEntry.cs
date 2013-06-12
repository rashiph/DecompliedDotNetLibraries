namespace System.Web.UI
{
    using System;
    using System.Web;

    internal class UserControlRegisterEntry : RegisterDirectiveEntry
    {
        private bool _comesFromConfig;
        private VirtualPath _source;
        private string _tagName;

        internal UserControlRegisterEntry(string tagPrefix, string tagName) : base(tagPrefix)
        {
            this._tagName = tagName;
        }

        internal bool ComesFromConfig
        {
            get
            {
                return this._comesFromConfig;
            }
            set
            {
                this._comesFromConfig = value;
            }
        }

        internal string Key
        {
            get
            {
                return (base.TagPrefix + ":" + this._tagName);
            }
        }

        internal string TagName
        {
            get
            {
                return this._tagName;
            }
        }

        internal VirtualPath UserControlSource
        {
            get
            {
                return this._source;
            }
            set
            {
                this._source = value;
            }
        }
    }
}

