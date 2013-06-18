namespace System.Web.UI
{
    using System;

    [Serializable]
    public sealed class IndexedString
    {
        private string _value;

        public IndexedString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException("s");
            }
            this._value = s;
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

