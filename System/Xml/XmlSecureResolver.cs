namespace System.Xml
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class XmlSecureResolver : XmlResolver
    {
        private PermissionSet permissionSet;
        private XmlResolver resolver;

        public XmlSecureResolver(XmlResolver resolver, PermissionSet permissionSet)
        {
            this.resolver = resolver;
            this.permissionSet = permissionSet;
        }

        public XmlSecureResolver(XmlResolver resolver, Evidence evidence) : this(resolver, SecurityManager.GetStandardSandbox(evidence))
        {
        }

        public XmlSecureResolver(XmlResolver resolver, string securityUrl) : this(resolver, CreateEvidenceForUrl(securityUrl))
        {
        }

        public static Evidence CreateEvidenceForUrl(string securityUrl)
        {
            Evidence evidence = new Evidence();
            if ((securityUrl != null) && (securityUrl.Length > 0))
            {
                evidence.AddHostEvidence<Url>(new Url(securityUrl));
                evidence.AddHostEvidence<Zone>(Zone.CreateFromUrl(securityUrl));
                Uri uri = new Uri(securityUrl, UriKind.RelativeOrAbsolute);
                if (uri.IsAbsoluteUri && !uri.IsFile)
                {
                    evidence.AddHostEvidence<Site>(Site.CreateFromUrl(securityUrl));
                }
                if (uri.IsAbsoluteUri && uri.IsUnc)
                {
                    string directoryName = Path.GetDirectoryName(uri.LocalPath);
                    if ((directoryName != null) && (directoryName.Length != 0))
                    {
                        evidence.AddHostEvidence<UncDirectory>(new UncDirectory(directoryName));
                    }
                }
            }
            return evidence;
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            this.permissionSet.PermitOnly();
            return this.resolver.GetEntity(absoluteUri, role, ofObjectToReturn);
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            return this.resolver.ResolveUri(baseUri, relativeUri);
        }

        public override ICredentials Credentials
        {
            set
            {
                this.resolver.Credentials = value;
            }
        }

        [Serializable]
        private class UncDirectory : EvidenceBase, IIdentityPermissionFactory
        {
            private string uncDir;

            public UncDirectory(string uncDirectory)
            {
                this.uncDir = uncDirectory;
            }

            public override EvidenceBase Clone()
            {
                return new XmlSecureResolver.UncDirectory(this.uncDir);
            }

            public IPermission CreateIdentityPermission(Evidence evidence)
            {
                return new FileIOPermission(FileIOPermissionAccess.Read, this.uncDir);
            }

            public override string ToString()
            {
                return this.ToXml().ToString();
            }

            private SecurityElement ToXml()
            {
                SecurityElement element = new SecurityElement("System.Xml.XmlSecureResolver");
                element.AddAttribute("version", "1");
                element.AddChild(new SecurityElement("UncDirectory", this.uncDir));
                return element;
            }
        }
    }
}

