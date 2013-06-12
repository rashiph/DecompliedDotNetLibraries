namespace System.Web.Compilation
{
    using System;

    public sealed class ImplicitResourceKey
    {
        private string _filter;
        private string _keyPrefix;
        private string _property;

        public ImplicitResourceKey()
        {
        }

        public ImplicitResourceKey(string filter, string keyPrefix, string property)
        {
            this._filter = filter;
            this._keyPrefix = keyPrefix;
            this._property = property;
        }

        public string Filter
        {
            get
            {
                return this._filter;
            }
            set
            {
                this._filter = value;
            }
        }

        public string KeyPrefix
        {
            get
            {
                return this._keyPrefix;
            }
            set
            {
                this._keyPrefix = value;
            }
        }

        public string Property
        {
            get
            {
                return this._property;
            }
            set
            {
                this._property = value;
            }
        }
    }
}

