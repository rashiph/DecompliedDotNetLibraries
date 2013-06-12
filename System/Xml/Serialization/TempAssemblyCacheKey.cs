namespace System.Xml.Serialization
{
    using System;

    internal class TempAssemblyCacheKey
    {
        private string ns;
        private object type;

        internal TempAssemblyCacheKey(string ns, object type)
        {
            this.type = type;
            this.ns = ns;
        }

        public override bool Equals(object o)
        {
            TempAssemblyCacheKey key = o as TempAssemblyCacheKey;
            if (key == null)
            {
                return false;
            }
            return ((key.type == this.type) && (key.ns == this.ns));
        }

        public override int GetHashCode()
        {
            return (((this.ns != null) ? this.ns.GetHashCode() : 0) ^ ((this.type != null) ? this.type.GetHashCode() : 0));
        }
    }
}

