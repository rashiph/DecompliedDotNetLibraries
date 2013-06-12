namespace System.Security.Policy
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Runtime.Hosting;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Util;
    using System.Threading;

    [ComVisible(true), SecurityCritical, SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public sealed class ApplicationSecurityInfo
    {
        private object m_appEvidence;
        private object m_appId;
        private ActivationContext m_context;
        private object m_defaultRequest;
        private object m_deployId;

        internal ApplicationSecurityInfo()
        {
        }

        public ApplicationSecurityInfo(ActivationContext activationContext)
        {
            if (activationContext == null)
            {
                throw new ArgumentNullException("activationContext");
            }
            this.m_context = activationContext;
        }

        private static System.ApplicationId ParseApplicationId(ICMS manifest)
        {
            if (manifest.Identity == null)
            {
                return null;
            }
            return new System.ApplicationId(Hex.DecodeHexString(manifest.Identity.GetAttribute("", "publicKeyToken")), manifest.Identity.GetAttribute("", "name"), new Version(manifest.Identity.GetAttribute("", "version")), manifest.Identity.GetAttribute("", "processorArchitecture"), manifest.Identity.GetAttribute("", "culture"));
        }

        public Evidence ApplicationEvidence
        {
            get
            {
                if (this.m_appEvidence == null)
                {
                    Evidence evidence = new Evidence();
                    if (this.m_context != null)
                    {
                        evidence = new Evidence();
                        Url url = new Url(this.m_context.Identity.CodeBase);
                        evidence.AddHostEvidence<Url>(url);
                        evidence.AddHostEvidence<Zone>(Zone.CreateFromUrl(this.m_context.Identity.CodeBase));
                        if (string.Compare("file:", 0, this.m_context.Identity.CodeBase, 0, 5, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            evidence.AddHostEvidence<Site>(Site.CreateFromUrl(this.m_context.Identity.CodeBase));
                        }
                        evidence.AddHostEvidence<StrongName>(new StrongName(new StrongNamePublicKeyBlob(this.DeploymentId.m_publicKeyToken), this.DeploymentId.Name, this.DeploymentId.Version));
                        evidence.AddHostEvidence<ActivationArguments>(new ActivationArguments(this.m_context));
                    }
                    Interlocked.CompareExchange(ref this.m_appEvidence, evidence, null);
                }
                return (this.m_appEvidence as Evidence);
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_appEvidence = value;
            }
        }

        public System.ApplicationId ApplicationId
        {
            get
            {
                if ((this.m_appId == null) && (this.m_context != null))
                {
                    System.ApplicationId id = ParseApplicationId(this.m_context.ApplicationComponentManifest);
                    Interlocked.CompareExchange(ref this.m_appId, id, null);
                }
                return (this.m_appId as System.ApplicationId);
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_appId = value;
            }
        }

        public PermissionSet DefaultRequestSet
        {
            get
            {
                if (this.m_defaultRequest == null)
                {
                    PermissionSet set = new PermissionSet(PermissionState.None);
                    if (this.m_context != null)
                    {
                        ICMS applicationComponentManifest = this.m_context.ApplicationComponentManifest;
                        string defaultPermissionSetID = ((IMetadataSectionEntry) applicationComponentManifest.MetadataSectionEntry).defaultPermissionSetID;
                        object ppUnknown = null;
                        if ((defaultPermissionSetID != null) && (defaultPermissionSetID.Length > 0))
                        {
                            ((ISectionWithStringKey) applicationComponentManifest.PermissionSetSection).Lookup(defaultPermissionSetID, out ppUnknown);
                            IPermissionSetEntry entry = ppUnknown as IPermissionSetEntry;
                            if (entry != null)
                            {
                                SecurityElement permissionSetXml = SecurityElement.FromString(entry.AllData.XmlSegment);
                                string str2 = permissionSetXml.Attribute("temp:Unrestricted");
                                if (str2 != null)
                                {
                                    permissionSetXml.AddAttribute("Unrestricted", str2);
                                }
                                if (string.Compare(permissionSetXml.Attribute("SameSite"), "Site", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    Url url = new Url(this.m_context.Identity.CodeBase);
                                    URLString uRLString = url.GetURLString();
                                    SecurityElement child = new NetCodeGroup(new AllMembershipCondition()).CreateWebPermission(uRLString.Host, uRLString.Scheme, uRLString.Port, "System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                                    if (child != null)
                                    {
                                        permissionSetXml.AddChild(child);
                                    }
                                    if (string.Compare("file:", 0, this.m_context.Identity.CodeBase, 0, 5, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        PolicyStatement statement = new FileCodeGroup(new AllMembershipCondition(), FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read).CalculatePolicy(url);
                                        if (statement != null)
                                        {
                                            PermissionSet permissionSet = statement.PermissionSet;
                                            if (permissionSet != null)
                                            {
                                                permissionSetXml.AddChild(permissionSet.GetPermission(typeof(FileIOPermission)).ToXml());
                                            }
                                        }
                                    }
                                }
                                set = new ReadOnlyPermissionSet(permissionSetXml);
                            }
                        }
                    }
                    Interlocked.CompareExchange(ref this.m_defaultRequest, set, null);
                }
                return (this.m_defaultRequest as PermissionSet);
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_defaultRequest = value;
            }
        }

        public System.ApplicationId DeploymentId
        {
            get
            {
                if ((this.m_deployId == null) && (this.m_context != null))
                {
                    System.ApplicationId id = ParseApplicationId(this.m_context.DeploymentComponentManifest);
                    Interlocked.CompareExchange(ref this.m_deployId, id, null);
                }
                return (this.m_deployId as System.ApplicationId);
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_deployId = value;
            }
        }
    }
}

