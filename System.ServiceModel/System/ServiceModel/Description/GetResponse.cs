namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    internal class GetResponse
    {
        private MetadataSet metadataSet;

        internal GetResponse()
        {
        }

        internal GetResponse(MetadataSet metadataSet) : this()
        {
            this.metadataSet = metadataSet;
        }

        [MessageBodyMember(Name="Metadata", Namespace="http://schemas.xmlsoap.org/ws/2004/09/mex")]
        internal MetadataSet Metadata
        {
            get
            {
                return this.metadataSet;
            }
            set
            {
                this.metadataSet = value;
            }
        }
    }
}

