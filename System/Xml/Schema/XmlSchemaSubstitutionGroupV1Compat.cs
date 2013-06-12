namespace System.Xml.Schema
{
    using System.Xml.Serialization;

    internal class XmlSchemaSubstitutionGroupV1Compat : XmlSchemaSubstitutionGroup
    {
        private XmlSchemaChoice choice = new XmlSchemaChoice();

        [XmlIgnore]
        internal XmlSchemaChoice Choice
        {
            get
            {
                return this.choice;
            }
        }
    }
}

