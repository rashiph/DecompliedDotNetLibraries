namespace System.Xml.Serialization
{
    using System;

    internal class SpecialMapping : TypeMapping
    {
        private bool namedAny;

        internal bool NamedAny
        {
            get
            {
                return this.namedAny;
            }
            set
            {
                this.namedAny = value;
            }
        }
    }
}

