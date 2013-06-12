namespace System.Xml.Schema
{
    using System;
    using System.Xml.Serialization;

    public class XmlSchemaAll : XmlSchemaGroupBase
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

        [XmlElement("element", typeof(XmlSchemaElement))]
        public override XmlSchemaObjectCollection Items
        {
            get
            {
                return this.items;
            }
        }
    }
}

