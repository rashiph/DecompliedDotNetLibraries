namespace System.Xml.Serialization
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    public class SoapAttributes
    {
        private SoapAttributeAttribute soapAttribute;
        private object soapDefaultValue;
        private SoapElementAttribute soapElement;
        private SoapEnumAttribute soapEnum;
        private bool soapIgnore;
        private SoapTypeAttribute soapType;

        public SoapAttributes()
        {
        }

        public SoapAttributes(ICustomAttributeProvider provider)
        {
            object[] customAttributes = provider.GetCustomAttributes(false);
            for (int i = 0; i < customAttributes.Length; i++)
            {
                if ((customAttributes[i] is SoapIgnoreAttribute) || (customAttributes[i] is ObsoleteAttribute))
                {
                    this.soapIgnore = true;
                    break;
                }
                if (customAttributes[i] is SoapElementAttribute)
                {
                    this.soapElement = (SoapElementAttribute) customAttributes[i];
                }
                else if (customAttributes[i] is SoapAttributeAttribute)
                {
                    this.soapAttribute = (SoapAttributeAttribute) customAttributes[i];
                }
                else if (customAttributes[i] is SoapTypeAttribute)
                {
                    this.soapType = (SoapTypeAttribute) customAttributes[i];
                }
                else if (customAttributes[i] is SoapEnumAttribute)
                {
                    this.soapEnum = (SoapEnumAttribute) customAttributes[i];
                }
                else if (customAttributes[i] is DefaultValueAttribute)
                {
                    this.soapDefaultValue = ((DefaultValueAttribute) customAttributes[i]).Value;
                }
            }
            if (this.soapIgnore)
            {
                this.soapElement = null;
                this.soapAttribute = null;
                this.soapType = null;
                this.soapEnum = null;
                this.soapDefaultValue = null;
            }
        }

        public SoapAttributeAttribute SoapAttribute
        {
            get
            {
                return this.soapAttribute;
            }
            set
            {
                this.soapAttribute = value;
            }
        }

        public object SoapDefaultValue
        {
            get
            {
                return this.soapDefaultValue;
            }
            set
            {
                this.soapDefaultValue = value;
            }
        }

        public SoapElementAttribute SoapElement
        {
            get
            {
                return this.soapElement;
            }
            set
            {
                this.soapElement = value;
            }
        }

        public SoapEnumAttribute SoapEnum
        {
            get
            {
                return this.soapEnum;
            }
            set
            {
                this.soapEnum = value;
            }
        }

        internal SoapAttributeFlags SoapFlags
        {
            get
            {
                SoapAttributeFlags flags = (SoapAttributeFlags) 0;
                if (this.soapElement != null)
                {
                    flags |= SoapAttributeFlags.Element;
                }
                if (this.soapAttribute != null)
                {
                    flags |= SoapAttributeFlags.Attribute;
                }
                if (this.soapEnum != null)
                {
                    flags |= SoapAttributeFlags.Enum;
                }
                if (this.soapType != null)
                {
                    flags |= SoapAttributeFlags.Type;
                }
                return flags;
            }
        }

        public bool SoapIgnore
        {
            get
            {
                return this.soapIgnore;
            }
            set
            {
                this.soapIgnore = value;
            }
        }

        public SoapTypeAttribute SoapType
        {
            get
            {
                return this.soapType;
            }
            set
            {
                this.soapType = value;
            }
        }
    }
}

