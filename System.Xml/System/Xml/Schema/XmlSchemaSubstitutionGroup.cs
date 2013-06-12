namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Xml;
    using System.Xml.Serialization;

    internal class XmlSchemaSubstitutionGroup : XmlSchemaObject
    {
        private XmlQualifiedName examplar = XmlQualifiedName.Empty;
        private ArrayList membersList = new ArrayList();

        [XmlIgnore]
        internal XmlQualifiedName Examplar
        {
            get
            {
                return this.examplar;
            }
            set
            {
                this.examplar = value;
            }
        }

        [XmlIgnore]
        internal ArrayList Members
        {
            get
            {
                return this.membersList;
            }
        }
    }
}

