namespace System.Security.Cryptography
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CngKeyCreationParameters
    {
        private CngExportPolicies? m_exportPolicy;
        private CngKeyCreationOptions m_keyCreationOptions;
        private CngKeyUsages? m_keyUsage;
        private CngPropertyCollection m_parameters = new CngPropertyCollection();
        private IntPtr m_parentWindowHandle;
        private CngProvider m_provider = CngProvider.MicrosoftSoftwareKeyStorageProvider;
        private CngUIPolicy m_uiPolicy;

        public CngExportPolicies? ExportPolicy
        {
            get
            {
                return this.m_exportPolicy;
            }
            set
            {
                this.m_exportPolicy = value;
            }
        }

        public CngKeyCreationOptions KeyCreationOptions
        {
            get
            {
                return this.m_keyCreationOptions;
            }
            set
            {
                this.m_keyCreationOptions = value;
            }
        }

        public CngKeyUsages? KeyUsage
        {
            get
            {
                return this.m_keyUsage;
            }
            set
            {
                this.m_keyUsage = value;
            }
        }

        public CngPropertyCollection Parameters
        {
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
            get
            {
                return this.m_parameters;
            }
        }

        internal CngPropertyCollection ParametersNoDemand
        {
            get
            {
                return this.m_parameters;
            }
        }

        public IntPtr ParentWindowHandle
        {
            get
            {
                return this.m_parentWindowHandle;
            }
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
            set
            {
                this.m_parentWindowHandle = value;
            }
        }

        public CngProvider Provider
        {
            get
            {
                return this.m_provider;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_provider = value;
            }
        }

        public CngUIPolicy UIPolicy
        {
            get
            {
                return this.m_uiPolicy;
            }
            [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.SafeSubWindows), HostProtection(SecurityAction.LinkDemand, Action=SecurityAction.Demand, UI=true)]
            set
            {
                this.m_uiPolicy = value;
            }
        }
    }
}

