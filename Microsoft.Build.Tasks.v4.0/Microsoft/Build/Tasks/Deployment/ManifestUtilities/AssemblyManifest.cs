namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [ComVisible(false), XmlRoot("AssemblyManifest")]
    public class AssemblyManifest : Manifest
    {
        private ProxyStub[] externalProxyStubs;

        [XmlIgnore]
        public ProxyStub[] ExternalProxyStubs
        {
            get
            {
                return this.externalProxyStubs;
            }
        }

        [XmlArray("ExternalProxyStubs"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public ProxyStub[] XmlExternalProxyStubs
        {
            get
            {
                return this.externalProxyStubs;
            }
            set
            {
                this.externalProxyStubs = value;
            }
        }
    }
}

