namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;

    public sealed class ServiceModelSectionGroup : ConfigurationSectionGroup
    {
        public static ServiceModelSectionGroup GetSectionGroup(System.Configuration.Configuration config)
        {
            if (config == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("config");
            }
            return (ServiceModelSectionGroup) config.SectionGroups["system.serviceModel"];
        }

        public BehaviorsSection Behaviors
        {
            get
            {
                return (BehaviorsSection) base.Sections["behaviors"];
            }
        }

        public BindingsSection Bindings
        {
            get
            {
                return (BindingsSection) base.Sections["bindings"];
            }
        }

        public ClientSection Client
        {
            get
            {
                return (ClientSection) base.Sections["client"];
            }
        }

        public ComContractsSection ComContracts
        {
            get
            {
                return (ComContractsSection) base.Sections["comContracts"];
            }
        }

        public CommonBehaviorsSection CommonBehaviors
        {
            get
            {
                return (CommonBehaviorsSection) base.Sections["commonBehaviors"];
            }
        }

        public DiagnosticSection Diagnostic
        {
            get
            {
                return (DiagnosticSection) base.Sections["diagnostics"];
            }
        }

        public ExtensionsSection Extensions
        {
            get
            {
                return (ExtensionsSection) base.Sections["extensions"];
            }
        }

        public ProtocolMappingSection ProtocolMapping
        {
            get
            {
                return (ProtocolMappingSection) base.Sections["protocolMapping"];
            }
        }

        public ServiceHostingEnvironmentSection ServiceHostingEnvironment
        {
            get
            {
                return (ServiceHostingEnvironmentSection) base.Sections["serviceHostingEnvironment"];
            }
        }

        public ServicesSection Services
        {
            get
            {
                return (ServicesSection) base.Sections["services"];
            }
        }

        public StandardEndpointsSection StandardEndpoints
        {
            get
            {
                return (StandardEndpointsSection) base.Sections["standardEndpoints"];
            }
        }
    }
}

