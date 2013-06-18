namespace System.Web.Services.Discovery
{
    using System;

    public class DiscoveryDocumentLinksPattern : DiscoverySearchPattern
    {
        public override DiscoveryReference GetDiscoveryReference(string filename)
        {
            return new DiscoveryDocumentReference(filename);
        }

        public override string Pattern
        {
            get
            {
                return "*.disco";
            }
        }
    }
}

