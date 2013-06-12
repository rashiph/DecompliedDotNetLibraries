namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public sealed class SoapQName : ISoapXsd
    {
        private string _key;
        private string _name;
        private string _namespace;

        public SoapQName()
        {
        }

        public SoapQName(string value)
        {
            this._name = value;
        }

        public SoapQName(string key, string name)
        {
            this._name = name;
            this._key = key;
        }

        public SoapQName(string key, string name, string namespaceValue)
        {
            this._name = name;
            this._namespace = namespaceValue;
            this._key = key;
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static SoapQName Parse(string value)
        {
            if (value == null)
            {
                return new SoapQName();
            }
            string key = "";
            string name = value;
            int index = value.IndexOf(':');
            if (index > 0)
            {
                key = value.Substring(0, index);
                name = value.Substring(index + 1);
            }
            return new SoapQName(key, name);
        }

        public override string ToString()
        {
            if ((this._key != null) && (this._key.Length != 0))
            {
                return (this._key + ":" + this._name);
            }
            return this._name;
        }

        public string Key
        {
            get
            {
                return this._key;
            }
            set
            {
                this._key = value;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public string Namespace
        {
            get
            {
                return this._namespace;
            }
            set
            {
                this._namespace = value;
            }
        }

        public static string XsdType
        {
            get
            {
                return "QName";
            }
        }
    }
}

