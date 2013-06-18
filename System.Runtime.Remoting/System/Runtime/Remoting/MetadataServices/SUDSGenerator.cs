namespace System.Runtime.Remoting.MetadataServices
{
    using System;
    using System.IO;

    internal class SUDSGenerator
    {
        private SdlType sdlType;
        private WsdlGenerator wsdlGenerator;

        internal SUDSGenerator(ServiceType[] serviceTypes, SdlType sdlType, TextWriter output)
        {
            this.wsdlGenerator = new WsdlGenerator(serviceTypes, sdlType, output);
            this.sdlType = sdlType;
        }

        internal SUDSGenerator(Type[] types, SdlType sdlType, TextWriter output)
        {
            this.wsdlGenerator = new WsdlGenerator(types, sdlType, output);
            this.sdlType = sdlType;
        }

        internal void Generate()
        {
            this.wsdlGenerator.Generate();
        }
    }
}

