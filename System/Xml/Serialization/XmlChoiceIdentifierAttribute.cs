namespace System.Xml.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false)]
    public class XmlChoiceIdentifierAttribute : Attribute
    {
        private string name;

        public XmlChoiceIdentifierAttribute()
        {
        }

        public XmlChoiceIdentifierAttribute(string name)
        {
            this.name = name;
        }

        public string MemberName
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                return string.Empty;
            }
            set
            {
                this.name = value;
            }
        }
    }
}

