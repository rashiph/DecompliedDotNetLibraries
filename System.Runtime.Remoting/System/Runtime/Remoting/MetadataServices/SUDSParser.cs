namespace System.Runtime.Remoting.MetadataServices
{
    using System;
    using System.Collections;
    using System.IO;

    internal class SUDSParser
    {
        private WsdlParser wsdlParser;

        internal SUDSParser(TextReader input, string outputDir, ArrayList outCodeStreamList, string locationURL, bool bWrappedProxy, string proxyNamespace)
        {
            this.wsdlParser = new WsdlParser(input, outputDir, outCodeStreamList, locationURL, bWrappedProxy, proxyNamespace);
        }

        internal void Parse()
        {
            this.wsdlParser.Parse();
        }
    }
}

