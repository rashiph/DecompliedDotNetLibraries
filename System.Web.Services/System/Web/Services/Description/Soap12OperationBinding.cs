namespace System.Web.Services.Description
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Web.Services.Configuration;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;

    [XmlFormatExtension("operation", "http://schemas.xmlsoap.org/wsdl/soap12/", typeof(OperationBinding))]
    public sealed class Soap12OperationBinding : SoapOperationBinding
    {
        private Soap12OperationBinding duplicateByRequestElement;
        private Soap12OperationBinding duplicateBySoapAction;
        private SoapReflectedMethod method;
        private bool soapActionRequired;

        internal Soap12OperationBinding DuplicateByRequestElement
        {
            get
            {
                return this.duplicateByRequestElement;
            }
            set
            {
                this.duplicateByRequestElement = value;
            }
        }

        internal Soap12OperationBinding DuplicateBySoapAction
        {
            get
            {
                return this.duplicateBySoapAction;
            }
            set
            {
                this.duplicateBySoapAction = value;
            }
        }

        internal SoapReflectedMethod Method
        {
            get
            {
                return this.method;
            }
            set
            {
                this.method = value;
            }
        }

        [XmlAttribute("soapActionRequired"), DefaultValue(false)]
        public bool SoapActionRequired
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.soapActionRequired;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.soapActionRequired = value;
            }
        }
    }
}

