namespace System.Xml.Schema
{
    using System;

    internal class ChameleonKey
    {
        internal Uri chameleonLocation;
        private int hashCode;
        internal XmlSchema originalSchema;
        internal string targetNS;

        public ChameleonKey(string ns, XmlSchema originalSchema)
        {
            this.targetNS = ns;
            this.chameleonLocation = originalSchema.BaseUri;
            if (this.chameleonLocation.OriginalString.Length == 0)
            {
                this.originalSchema = originalSchema;
            }
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            ChameleonKey key = obj as ChameleonKey;
            if (key == null)
            {
                return false;
            }
            return ((this.targetNS.Equals(key.targetNS) && this.chameleonLocation.Equals(key.chameleonLocation)) && object.ReferenceEquals(this.originalSchema, key.originalSchema));
        }

        public override int GetHashCode()
        {
            if (this.hashCode == 0)
            {
                this.hashCode = (this.targetNS.GetHashCode() + this.chameleonLocation.GetHashCode()) + ((this.originalSchema == null) ? 0 : this.originalSchema.GetHashCode());
            }
            return this.hashCode;
        }
    }
}

