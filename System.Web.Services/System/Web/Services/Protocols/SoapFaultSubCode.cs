namespace System.Web.Services.Protocols
{
    using System;
    using System.Runtime;
    using System.Xml;

    [Serializable]
    public class SoapFaultSubCode
    {
        private XmlQualifiedName code;
        private SoapFaultSubCode subCode;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SoapFaultSubCode(XmlQualifiedName code) : this(code, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SoapFaultSubCode(XmlQualifiedName code, SoapFaultSubCode subCode)
        {
            this.code = code;
            this.subCode = subCode;
        }

        public XmlQualifiedName Code
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.code;
            }
        }

        public SoapFaultSubCode SubCode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.subCode;
            }
        }
    }
}

