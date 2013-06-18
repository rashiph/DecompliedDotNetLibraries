namespace System.Web.Services.Description
{
    using System.Web.Services.Configuration;
    using System.Xml.Serialization;

    [XmlFormatExtensionPoint("Extensions")]
    public sealed class MimePart : ServiceDescriptionFormatExtension
    {
        private ServiceDescriptionFormatExtensionCollection extensions;

        [XmlIgnore]
        public ServiceDescriptionFormatExtensionCollection Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new ServiceDescriptionFormatExtensionCollection(this);
                }
                return this.extensions;
            }
        }
    }
}

