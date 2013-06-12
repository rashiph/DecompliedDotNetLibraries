namespace System.Xml.Serialization
{
    using System;
    using System.Xml;

    public class SoapSchemaMember
    {
        private string memberName;
        private XmlQualifiedName type = XmlQualifiedName.Empty;

        public string MemberName
        {
            get
            {
                if (this.memberName != null)
                {
                    return this.memberName;
                }
                return string.Empty;
            }
            set
            {
                this.memberName = value;
            }
        }

        public XmlQualifiedName MemberType
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
    }
}

