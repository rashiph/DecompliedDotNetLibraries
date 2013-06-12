namespace System.Runtime.Remoting.Metadata
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;

    [AttributeUsage(AttributeTargets.Method), ComVisible(true)]
    public sealed class SoapMethodAttribute : SoapAttribute
    {
        private bool _bSoapActionExplicitySet;
        private string _responseXmlElementName;
        private string _responseXmlNamespace;
        private string _returnXmlElementName;
        private string _SoapAction;

        public string ResponseXmlElementName
        {
            get
            {
                if ((this._responseXmlElementName == null) && (base.ReflectInfo != null))
                {
                    this._responseXmlElementName = ((MemberInfo) base.ReflectInfo).Name + "Response";
                }
                return this._responseXmlElementName;
            }
            set
            {
                this._responseXmlElementName = value;
            }
        }

        public string ResponseXmlNamespace
        {
            get
            {
                if (this._responseXmlNamespace == null)
                {
                    this._responseXmlNamespace = this.XmlNamespace;
                }
                return this._responseXmlNamespace;
            }
            set
            {
                this._responseXmlNamespace = value;
            }
        }

        public string ReturnXmlElementName
        {
            get
            {
                if (this._returnXmlElementName == null)
                {
                    this._returnXmlElementName = "return";
                }
                return this._returnXmlElementName;
            }
            set
            {
                this._returnXmlElementName = value;
            }
        }

        public string SoapAction
        {
            [SecuritySafeCritical]
            get
            {
                if (this._SoapAction == null)
                {
                    this._SoapAction = this.XmlTypeNamespaceOfDeclaringType + "#" + ((MemberInfo) base.ReflectInfo).Name;
                }
                return this._SoapAction;
            }
            set
            {
                this._SoapAction = value;
                this._bSoapActionExplicitySet = true;
            }
        }

        internal bool SoapActionExplicitySet
        {
            get
            {
                return this._bSoapActionExplicitySet;
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

        public override string XmlNamespace
        {
            [SecuritySafeCritical]
            get
            {
                if (base.ProtXmlNamespace == null)
                {
                    base.ProtXmlNamespace = this.XmlTypeNamespaceOfDeclaringType;
                }
                return base.ProtXmlNamespace;
            }
            set
            {
                base.ProtXmlNamespace = value;
            }
        }

        private string XmlTypeNamespaceOfDeclaringType
        {
            [SecurityCritical]
            get
            {
                if (base.ReflectInfo != null)
                {
                    return XmlNamespaceEncoder.GetXmlNamespaceForType((RuntimeType) ((MemberInfo) base.ReflectInfo).DeclaringType, null);
                }
                return null;
            }
        }
    }
}

