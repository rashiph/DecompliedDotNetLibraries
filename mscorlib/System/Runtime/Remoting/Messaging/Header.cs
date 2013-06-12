namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public class Header
    {
        public string HeaderNamespace;
        public bool MustUnderstand;
        public string Name;
        public object Value;

        public Header(string _Name, object _Value) : this(_Name, _Value, true)
        {
        }

        public Header(string _Name, object _Value, bool _MustUnderstand)
        {
            this.Name = _Name;
            this.Value = _Value;
            this.MustUnderstand = _MustUnderstand;
        }

        public Header(string _Name, object _Value, bool _MustUnderstand, string _HeaderNamespace)
        {
            this.Name = _Name;
            this.Value = _Value;
            this.MustUnderstand = _MustUnderstand;
            this.HeaderNamespace = _HeaderNamespace;
        }
    }
}

