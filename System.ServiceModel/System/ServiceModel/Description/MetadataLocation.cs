namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel;
    using System.Xml.Serialization;

    [XmlRoot(ElementName="Location", Namespace="http://schemas.xmlsoap.org/ws/2004/09/mex")]
    public class MetadataLocation
    {
        private string location;

        public MetadataLocation()
        {
        }

        public MetadataLocation(string location)
        {
            this.Location = location;
        }

        [XmlText]
        public string Location
        {
            get
            {
                return this.location;
            }
            set
            {
                Uri uri;
                if ((value != null) && !Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out uri))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxMetadataReferenceInvalidLocation", new object[] { value }));
                }
                this.location = value;
            }
        }
    }
}

