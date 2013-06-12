namespace System.Xml.Schema
{
    using System;
    using System.Xml.Serialization;

    public class XmlSchemaSequence : XmlSchemaGroupBase
    {
        private XmlSchemaObjectCollection items = new XmlSchemaObjectCollection();

        internal override void SetItems(XmlSchemaObjectCollection newItems)
        {
            this.items = newItems;
        }

        internal override bool IsEmpty
        {
            get
            {
                if (!base.IsEmpty)
                {
                    return (this.items.Count == 0);
                }
                return true;
            }
        }

        [XmlElement("group", typeof(XmlSchemaGroupRef)), XmlElement("any", typeof(XmlSchemaAny)), XmlElement("element", typeof(XmlSchemaElement)), XmlElement("choice", typeof(XmlSchemaChoice)), XmlElement("sequence", typeof(XmlSchemaSequence))]
        public override XmlSchemaObjectCollection Items
        {
            get
            {
                return this.items;
            }
        }
    }
}

