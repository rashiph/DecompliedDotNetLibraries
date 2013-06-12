namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false), ComVisible(true)]
    public sealed class HostProtectionAttribute : CodeAccessSecurityAttribute
    {
        private HostProtectionResource m_resources;

        [SecuritySafeCritical]
        public HostProtectionAttribute() : base(SecurityAction.LinkDemand)
        {
        }

        public HostProtectionAttribute(SecurityAction action) : base(action)
        {
            if (action != SecurityAction.LinkDemand)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"));
            }
        }

        public override IPermission CreatePermission()
        {
            if (base.m_unrestricted)
            {
                return new HostProtectionPermission(PermissionState.Unrestricted);
            }
            return new HostProtectionPermission(this.m_resources);
        }

        public bool ExternalProcessMgmt
        {
            get
            {
                return ((this.m_resources & HostProtectionResource.ExternalProcessMgmt) != HostProtectionResource.None);
            }
            set
            {
                this.m_resources = value ? (this.m_resources | HostProtectionResource.ExternalProcessMgmt) : (this.m_resources & ~HostProtectionResource.ExternalProcessMgmt);
            }
        }

        public bool ExternalThreading
        {
            get
            {
                return ((this.m_resources & HostProtectionResource.ExternalThreading) != HostProtectionResource.None);
            }
            set
            {
                this.m_resources = value ? (this.m_resources | HostProtectionResource.ExternalThreading) : (this.m_resources & ~HostProtectionResource.ExternalThreading);
            }
        }

        public bool MayLeakOnAbort
        {
            get
            {
                return ((this.m_resources & HostProtectionResource.MayLeakOnAbort) != HostProtectionResource.None);
            }
            set
            {
                this.m_resources = value ? (this.m_resources | HostProtectionResource.MayLeakOnAbort) : (this.m_resources & ~HostProtectionResource.MayLeakOnAbort);
            }
        }

        public HostProtectionResource Resources
        {
            get
            {
                return this.m_resources;
            }
            set
            {
                this.m_resources = value;
            }
        }

        [ComVisible(true)]
        public bool SecurityInfrastructure
        {
            get
            {
                return ((this.m_resources & HostProtectionResource.SecurityInfrastructure) != HostProtectionResource.None);
            }
            set
            {
                this.m_resources = value ? (this.m_resources | HostProtectionResource.SecurityInfrastructure) : (this.m_resources & ~HostProtectionResource.SecurityInfrastructure);
            }
        }

        public bool SelfAffectingProcessMgmt
        {
            get
            {
                return ((this.m_resources & HostProtectionResource.SelfAffectingProcessMgmt) != HostProtectionResource.None);
            }
            set
            {
                this.m_resources = value ? (this.m_resources | HostProtectionResource.SelfAffectingProcessMgmt) : (this.m_resources & ~HostProtectionResource.SelfAffectingProcessMgmt);
            }
        }

        public bool SelfAffectingThreading
        {
            get
            {
                return ((this.m_resources & HostProtectionResource.SelfAffectingThreading) != HostProtectionResource.None);
            }
            set
            {
                this.m_resources = value ? (this.m_resources | HostProtectionResource.SelfAffectingThreading) : (this.m_resources & ~HostProtectionResource.SelfAffectingThreading);
            }
        }

        public bool SharedState
        {
            get
            {
                return ((this.m_resources & HostProtectionResource.SharedState) != HostProtectionResource.None);
            }
            set
            {
                this.m_resources = value ? (this.m_resources | HostProtectionResource.SharedState) : (this.m_resources & ~HostProtectionResource.SharedState);
            }
        }

        public bool Synchronization
        {
            get
            {
                return ((this.m_resources & HostProtectionResource.Synchronization) != HostProtectionResource.None);
            }
            set
            {
                this.m_resources = value ? (this.m_resources | HostProtectionResource.Synchronization) : (this.m_resources & ~HostProtectionResource.Synchronization);
            }
        }

        public bool UI
        {
            get
            {
                return ((this.m_resources & HostProtectionResource.UI) != HostProtectionResource.None);
            }
            set
            {
                this.m_resources = value ? (this.m_resources | HostProtectionResource.UI) : (this.m_resources & ~HostProtectionResource.UI);
            }
        }
    }
}

