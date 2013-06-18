namespace System.Web.Services.Description
{
    using System;
    using System.Runtime;
    using System.Web.Services.Configuration;
    using System.Xml.Serialization;

    [XmlFormatExtension("mimeXml", "http://schemas.xmlsoap.org/wsdl/mime/", typeof(MimePart), typeof(InputBinding), typeof(OutputBinding))]
    public sealed class MimeXmlBinding : ServiceDescriptionFormatExtension
    {
        private string part;

        [XmlAttribute("part")]
        public string Part
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.part;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.part = value;
            }
        }
    }
}

