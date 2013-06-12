namespace System.Xml.Serialization
{
    using System;

    internal class PrimitiveMapping : TypeMapping
    {
        private bool isList;

        internal override bool IsList
        {
            get
            {
                return this.isList;
            }
            set
            {
                this.isList = value;
            }
        }
    }
}

