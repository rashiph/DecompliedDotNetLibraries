namespace System.Xml.Schema
{
    using System;
    using System.Xml.Serialization;

    public abstract class XmlSchemaGroupBase : XmlSchemaParticle
    {
        protected XmlSchemaGroupBase()
        {
        }

        internal abstract void SetItems(XmlSchemaObjectCollection newItems);

        [XmlIgnore]
        public abstract XmlSchemaObjectCollection Items { get; }
    }
}

