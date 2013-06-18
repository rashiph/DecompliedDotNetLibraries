namespace System.Web.Services.Description
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Web.Services.Configuration;
    using System.Xml.Serialization;

    [XmlFormatExtension("operation", "http://schemas.xmlsoap.org/wsdl/soap/", typeof(OperationBinding))]
    public class SoapOperationBinding : ServiceDescriptionFormatExtension
    {
        private string soapAction;
        private SoapBindingStyle style;

        [XmlAttribute("soapAction")]
        public string SoapAction
        {
            get
            {
                if (this.soapAction != null)
                {
                    return this.soapAction;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.soapAction = value;
            }
        }

        [XmlAttribute("style"), DefaultValue(0)]
        public SoapBindingStyle Style
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.style;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.style = value;
            }
        }
    }
}

