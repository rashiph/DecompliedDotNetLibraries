namespace System.Xml.Serialization
{
    using System;

    public class XmlReflectionMember
    {
        private bool isReturnValue;
        private string memberName;
        private bool overrideIsNullable;
        private System.Xml.Serialization.SoapAttributes soapAttributes = new System.Xml.Serialization.SoapAttributes();
        private Type type;
        private System.Xml.Serialization.XmlAttributes xmlAttributes = new System.Xml.Serialization.XmlAttributes();

        public bool IsReturnValue
        {
            get
            {
                return this.isReturnValue;
            }
            set
            {
                this.isReturnValue = value;
            }
        }

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

        public Type MemberType
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

        public bool OverrideIsNullable
        {
            get
            {
                return this.overrideIsNullable;
            }
            set
            {
                this.overrideIsNullable = value;
            }
        }

        public System.Xml.Serialization.SoapAttributes SoapAttributes
        {
            get
            {
                return this.soapAttributes;
            }
            set
            {
                this.soapAttributes = value;
            }
        }

        public System.Xml.Serialization.XmlAttributes XmlAttributes
        {
            get
            {
                return this.xmlAttributes;
            }
            set
            {
                this.xmlAttributes = value;
            }
        }
    }
}

