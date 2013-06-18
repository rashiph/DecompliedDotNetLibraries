namespace System.DirectoryServices.Protocols
{
    using System;
    using System.DirectoryServices;
    using System.Security.Permissions;
    using System.Xml;

    public abstract class DsmlSoapConnection : DirectoryConnection
    {
        internal XmlNode soapHeaders;

        protected DsmlSoapConnection()
        {
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public abstract void BeginSession();
        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public abstract void EndSession();

        public abstract string SessionId { get; }

        public XmlNode SoapRequestHeader
        {
            get
            {
                return this.soapHeaders;
            }
            set
            {
                this.soapHeaders = value;
            }
        }
    }
}

