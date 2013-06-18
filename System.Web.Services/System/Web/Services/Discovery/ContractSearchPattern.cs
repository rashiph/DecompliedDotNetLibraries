namespace System.Web.Services.Discovery
{
    using System;

    public sealed class ContractSearchPattern : DiscoverySearchPattern
    {
        public override DiscoveryReference GetDiscoveryReference(string filename)
        {
            return new ContractReference(filename + "?wsdl", filename);
        }

        public override string Pattern
        {
            get
            {
                return "*.asmx";
            }
        }
    }
}

