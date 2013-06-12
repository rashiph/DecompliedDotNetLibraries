namespace System.Web.Caching
{
    using System;

    [Serializable]
    public sealed class HeaderElement
    {
        private string _name;
        private string _value;

        private HeaderElement()
        {
        }

        public HeaderElement(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this._name = name;
            this._value = value;
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public string Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

