namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;

    internal class ComPlusServiceLoader
    {
        private ConfigLoader configLoader;
        private ServiceInfo info;
        private ComPlusTypeLoader typeLoader;

        public ComPlusServiceLoader(ServiceInfo info)
        {
            this.info = info;
            this.typeLoader = new ComPlusTypeLoader(info);
            this.configLoader = new ConfigLoader(this.typeLoader);
        }

        private void AddBehaviors(System.ServiceModel.Description.ServiceDescription service)
        {
            ServiceBehaviorAttribute attribute = this.EnsureBehaviorAttribute(service);
            attribute.InstanceProvider = new ComPlusInstanceProvider(this.info);
            attribute.InstanceContextMode = InstanceContextMode.Single;
            attribute.ConcurrencyMode = ConcurrencyMode.Multiple;
            attribute.UseSynchronizationContext = false;
            service.Behaviors.Add(new SecurityCookieModeValidator());
            if (AspNetEnvironment.Enabled && (service.Behaviors.Find<AspNetCompatibilityRequirementsAttribute>() == null))
            {
                AspNetCompatibilityRequirementsAttribute item = new AspNetCompatibilityRequirementsAttribute();
                service.Behaviors.Add(item);
            }
        }

        private ServiceBehaviorAttribute EnsureBehaviorAttribute(System.ServiceModel.Description.ServiceDescription service)
        {
            if (service.Behaviors.Contains(typeof(ServiceBehaviorAttribute)))
            {
                return (ServiceBehaviorAttribute) service.Behaviors[typeof(ServiceBehaviorAttribute)];
            }
            ServiceBehaviorAttribute item = new ServiceBehaviorAttribute();
            service.Behaviors.Insert(0, item);
            return item;
        }

        public System.ServiceModel.Description.ServiceDescription Load(ServiceHostBase host)
        {
            System.ServiceModel.Description.ServiceDescription service = new System.ServiceModel.Description.ServiceDescription(this.info.ServiceName);
            this.AddBehaviors(service);
            this.configLoader.LoadServiceDescription(host, service, this.info.ServiceElement, new Action<Uri>(host.LoadConfigurationSectionHelper));
            this.ValidateConfigInstanceSettings(service);
            ComPlusServiceHostTrace.Trace(TraceEventType.Information, 0x50005, "TraceCodeComIntegrationServiceHostCreatedServiceEndpoint", this.info, service.Endpoints);
            return service;
        }

        private void ValidateConfigInstanceSettings(System.ServiceModel.Description.ServiceDescription service)
        {
            ServiceBehaviorAttribute attribute = this.EnsureBehaviorAttribute(service);
            foreach (ServiceEndpoint endpoint in service.Endpoints)
            {
                if ((endpoint != null) && !endpoint.InternalIsSystemEndpoint(service))
                {
                    if (endpoint.Contract.SessionMode == SessionMode.Required)
                    {
                        if (attribute.InstanceContextMode == InstanceContextMode.PerCall)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.InconsistentSessionRequirements());
                        }
                        attribute.InstanceContextMode = InstanceContextMode.PerSession;
                    }
                    else
                    {
                        if (attribute.InstanceContextMode == InstanceContextMode.PerSession)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.InconsistentSessionRequirements());
                        }
                        attribute.InstanceContextMode = InstanceContextMode.PerCall;
                    }
                }
            }
            if (attribute.InstanceContextMode == InstanceContextMode.Single)
            {
                attribute.InstanceContextMode = InstanceContextMode.PerSession;
            }
        }
    }
}

