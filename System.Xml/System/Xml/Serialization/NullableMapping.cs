namespace System.Xml.Serialization
{
    using System;

    internal class NullableMapping : TypeMapping
    {
        private TypeMapping baseMapping;

        internal TypeMapping BaseMapping
        {
            get
            {
                return this.baseMapping;
            }
            set
            {
                this.baseMapping = value;
            }
        }

        internal override string DefaultElementName
        {
            get
            {
                return this.BaseMapping.DefaultElementName;
            }
        }
    }
}

