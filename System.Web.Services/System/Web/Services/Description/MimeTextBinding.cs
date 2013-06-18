namespace System.Web.Services.Description
{
    using System;
    using System.Runtime;
    using System.Web.Services.Configuration;
    using System.Xml.Serialization;

    [XmlFormatExtension("text", "http://microsoft.com/wsdl/mime/textMatching/", typeof(InputBinding), typeof(OutputBinding), typeof(MimePart)), XmlFormatExtensionPrefix("tm", "http://microsoft.com/wsdl/mime/textMatching/")]
    public sealed class MimeTextBinding : ServiceDescriptionFormatExtension
    {
        private MimeTextMatchCollection matches = new MimeTextMatchCollection();
        public const string Namespace = "http://microsoft.com/wsdl/mime/textMatching/";

        [XmlElement("match", typeof(MimeTextMatch))]
        public MimeTextMatchCollection Matches
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.matches;
            }
        }
    }
}

