namespace System.Web.Services.Discovery
{
    using System;

    public sealed class DiscoveryDocumentSearchPattern : DiscoverySearchPattern
    {
        public override DiscoveryReference GetDiscoveryReference(string filename)
        {
            return new DiscoveryDocumentReference(filename);
        }

        public override string Pattern
        {
            get
            {
                return "*.vsdisco";
            }
        }
    }
}

