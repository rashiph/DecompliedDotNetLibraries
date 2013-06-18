namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Xml;

    internal static class XmlNamespaces
    {
        public const string asmv1 = "urn:schemas-microsoft-com:asm.v1";
        public const string asmv2 = "urn:schemas-microsoft-com:asm.v2";
        public const string asmv3 = "urn:schemas-microsoft-com:asm.v3";
        public const string dsig = "http://www.w3.org/2000/09/xmldsig#";
        public const string xrml = "urn:mpeg:mpeg21:2003:01-REL-R-NS";
        public const string xsi = "http://www.w3.org/2001/XMLSchema-instance";

        public static XmlNamespaceManager GetNamespaceManager(XmlNameTable nameTable)
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(nameTable);
            manager.AddNamespace("asmv1", "urn:schemas-microsoft-com:asm.v1");
            manager.AddNamespace("asmv2", "urn:schemas-microsoft-com:asm.v2");
            manager.AddNamespace("asmv3", "urn:schemas-microsoft-com:asm.v3");
            manager.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");
            manager.AddNamespace("xrml", "urn:mpeg:mpeg21:2003:01-REL-R-NS");
            manager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            return manager;
        }
    }
}

