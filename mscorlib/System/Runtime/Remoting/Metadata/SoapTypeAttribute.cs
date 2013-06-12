namespace System.Runtime.Remoting.Metadata
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class), ComVisible(true)]
    public sealed class SoapTypeAttribute : SoapAttribute
    {
        private ExplicitlySet _explicitlySet;
        private SoapOption _SoapOptions;
        private string _XmlElementName;
        private XmlFieldOrderOption _XmlFieldOrder;
        private string _XmlTypeName;
        private string _XmlTypeNamespace;

        private static string GetTypeName(Type t)
        {
            if (!t.IsNested)
            {
                return t.Name;
            }
            string fullName = t.FullName;
            string str2 = t.Namespace;
            if ((str2 != null) && (str2.Length != 0))
            {
                return fullName.Substring(str2.Length + 1);
            }
            return fullName;
        }

        internal bool IsInteropXmlElement()
        {
            return ((this._explicitlySet & (ExplicitlySet.XmlNamespace | ExplicitlySet.XmlElementName)) != ExplicitlySet.None);
        }

        internal bool IsInteropXmlType()
        {
            return ((this._explicitlySet & (ExplicitlySet.XmlTypeNamespace | ExplicitlySet.XmlTypeName)) != ExplicitlySet.None);
        }

        public SoapOption SoapOptions
        {
            get
            {
                return this._SoapOptions;
            }
            set
            {
                this._SoapOptions = value;
            }
        }

        public override bool UseAttribute
        {
            get
            {
                return false;
            }
            set
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Attribute_UseAttributeNotsettable"));
            }
        }

        public string XmlElementName
        {
            get
            {
                if ((this._XmlElementName == null) && (base.ReflectInfo != null))
                {
                    this._XmlElementName = GetTypeName((Type) base.ReflectInfo);
                }
                return this._XmlElementName;
            }
            set
            {
                this._XmlElementName = value;
                this._explicitlySet |= ExplicitlySet.XmlElementName;
            }
        }

        public XmlFieldOrderOption XmlFieldOrder
        {
            get
            {
                return this._XmlFieldOrder;
            }
            set
            {
                this._XmlFieldOrder = value;
            }
        }

        public override string XmlNamespace
        {
            [SecuritySafeCritical]
            get
            {
                if ((base.ProtXmlNamespace == null) && (base.ReflectInfo != null))
                {
                    base.ProtXmlNamespace = this.XmlTypeNamespace;
                }
                return base.ProtXmlNamespace;
            }
            set
            {
                base.ProtXmlNamespace = value;
                this._explicitlySet |= ExplicitlySet.XmlNamespace;
            }
        }

        public string XmlTypeName
        {
            get
            {
                if ((this._XmlTypeName == null) && (base.ReflectInfo != null))
                {
                    this._XmlTypeName = GetTypeName((Type) base.ReflectInfo);
                }
                return this._XmlTypeName;
            }
            set
            {
                this._XmlTypeName = value;
                this._explicitlySet |= ExplicitlySet.XmlTypeName;
            }
        }

        public string XmlTypeNamespace
        {
            [SecuritySafeCritical]
            get
            {
                if ((this._XmlTypeNamespace == null) && (base.ReflectInfo != null))
                {
                    this._XmlTypeNamespace = XmlNamespaceEncoder.GetXmlNamespaceForTypeNamespace((RuntimeType) base.ReflectInfo, null);
                }
                return this._XmlTypeNamespace;
            }
            set
            {
                this._XmlTypeNamespace = value;
                this._explicitlySet |= ExplicitlySet.XmlTypeNamespace;
            }
        }

        [Serializable, Flags]
        private enum ExplicitlySet
        {
            None = 0,
            XmlElementName = 1,
            XmlNamespace = 2,
            XmlTypeName = 4,
            XmlTypeNamespace = 8
        }
    }
}

