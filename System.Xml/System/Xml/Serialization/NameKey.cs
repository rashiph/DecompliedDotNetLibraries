namespace System.Xml.Serialization
{
    using System;

    internal class NameKey
    {
        private string name;
        private string ns;

        internal NameKey(string name, string ns)
        {
            this.name = name;
            this.ns = ns;
        }

        public override bool Equals(object other)
        {
            if (!(other is NameKey))
            {
                return false;
            }
            NameKey key = (NameKey) other;
            return ((this.name == key.name) && (this.ns == key.ns));
        }

        public override int GetHashCode()
        {
            return (((this.ns == null) ? "<null>".GetHashCode() : this.ns.GetHashCode()) ^ ((this.name == null) ? 0 : this.name.GetHashCode()));
        }
    }
}

