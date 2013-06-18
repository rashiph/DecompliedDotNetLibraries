namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    [DebuggerDisplay("ServiceType={serviceType}")]
    public class ServiceDescription
    {
        private KeyedByTypeCollection<IServiceBehavior> behaviors;
        private string configurationName;
        private ServiceEndpointCollection endpoints;
        private XmlName serviceName;
        private string serviceNamespace;
        private Type serviceType;

        public ServiceDescription()
        {
            this.behaviors = new KeyedByTypeCollection<IServiceBehavior>();
            this.endpoints = new ServiceEndpointCollection();
            this.serviceNamespace = "http://tempuri.org/";
        }

        public ServiceDescription(IEnumerable<ServiceEndpoint> endpoints) : this()
        {
            if (endpoints == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoints");
            }
            foreach (ServiceEndpoint endpoint in endpoints)
            {
                this.endpoints.Add(endpoint);
            }
        }

        internal ServiceDescription(string serviceName)
        {
            this.behaviors = new KeyedByTypeCollection<IServiceBehavior>();
            this.endpoints = new ServiceEndpointCollection();
            this.serviceNamespace = "http://tempuri.org/";
            if (string.IsNullOrEmpty(serviceName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceName");
            }
            this.Name = serviceName;
        }

        private static void AddBehaviors(System.ServiceModel.Description.ServiceDescription serviceDescription)
        {
            Type serviceType = serviceDescription.ServiceType;
            TypeLoader.ApplyServiceInheritance<IServiceBehavior, KeyedByTypeCollection<IServiceBehavior>>(serviceType, serviceDescription.Behaviors, new TypeLoader.ServiceInheritanceCallback<IServiceBehavior, KeyedByTypeCollection<IServiceBehavior>>(System.ServiceModel.Description.ServiceDescription.GetIServiceBehaviorAttributes));
            ServiceBehaviorAttribute attribute = EnsureBehaviorAttribute(serviceDescription);
            if (attribute.Name != null)
            {
                serviceDescription.Name = new XmlName(attribute.Name).EncodedName;
            }
            if (attribute.Namespace != null)
            {
                serviceDescription.Namespace = attribute.Namespace;
            }
            if (string.IsNullOrEmpty(attribute.ConfigurationName))
            {
                serviceDescription.ConfigurationName = serviceType.FullName;
            }
            else
            {
                serviceDescription.ConfigurationName = attribute.ConfigurationName;
            }
            AspNetEnvironment.Current.EnsureCompatibilityRequirements(serviceDescription);
        }

        internal static object CreateImplementation(Type serviceType)
        {
            object obj3;
            ConstructorInfo info = serviceType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (info == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxNoDefaultConstructor")));
            }
            try
            {
                obj3 = info.Invoke(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture);
            }
            catch (MethodAccessException exception)
            {
                SecurityException innerException = exception.InnerException as SecurityException;
                if ((innerException == null) || !innerException.PermissionType.Equals(typeof(ReflectionPermission)))
                {
                    throw;
                }
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.ServiceModel.SR.GetString("PartialTrustServiceCtorNotVisible", new object[] { serviceType.FullName })));
            }
            return obj3;
        }

        private static ServiceBehaviorAttribute EnsureBehaviorAttribute(System.ServiceModel.Description.ServiceDescription description)
        {
            ServiceBehaviorAttribute item = description.Behaviors.Find<ServiceBehaviorAttribute>();
            if (item == null)
            {
                item = new ServiceBehaviorAttribute();
                description.Behaviors.Insert(0, item);
            }
            return item;
        }

        internal void EnsureInvariants()
        {
            for (int i = 0; i < this.Endpoints.Count; i++)
            {
                ServiceEndpoint endpoint = this.Endpoints[i];
                if (endpoint == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AChannelServiceEndpointIsNull0")));
                }
                endpoint.EnsureInvariants();
            }
        }

        private static void GetIServiceBehaviorAttributes(Type currentServiceType, KeyedByTypeCollection<IServiceBehavior> behaviors)
        {
            foreach (IServiceBehavior behavior in ServiceReflector.GetCustomAttributes(currentServiceType, typeof(IServiceBehavior)))
            {
                behaviors.Add(behavior);
            }
        }

        public static System.ServiceModel.Description.ServiceDescription GetService(object serviceImplementation)
        {
            if (serviceImplementation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceImplementation");
            }
            Type type = serviceImplementation.GetType();
            System.ServiceModel.Description.ServiceDescription serviceDescription = new System.ServiceModel.Description.ServiceDescription {
                ServiceType = type
            };
            if (serviceImplementation is IServiceBehavior)
            {
                serviceDescription.Behaviors.Add((IServiceBehavior) serviceImplementation);
            }
            AddBehaviors(serviceDescription);
            SetupSingleton(serviceDescription, serviceImplementation, true);
            return serviceDescription;
        }

        public static System.ServiceModel.Description.ServiceDescription GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceType");
            }
            if (!serviceType.IsClass)
            {
                throw new ArgumentException(System.ServiceModel.SR.GetString("SFxServiceHostNeedsClass"));
            }
            System.ServiceModel.Description.ServiceDescription serviceDescription = new System.ServiceModel.Description.ServiceDescription {
                ServiceType = serviceType
            };
            AddBehaviors(serviceDescription);
            SetupSingleton(serviceDescription, null, false);
            return serviceDescription;
        }

        private static void SetupSingleton(System.ServiceModel.Description.ServiceDescription serviceDescription, object implementation, bool isWellKnown)
        {
            ServiceBehaviorAttribute attribute = EnsureBehaviorAttribute(serviceDescription);
            Type serviceType = serviceDescription.ServiceType;
            if ((implementation == null) && (attribute.InstanceContextMode == InstanceContextMode.Single))
            {
                implementation = CreateImplementation(serviceType);
            }
            if (isWellKnown)
            {
                attribute.SetWellKnownSingleton(implementation);
            }
            else if ((implementation != null) && (attribute.InstanceContextMode == InstanceContextMode.Single))
            {
                attribute.SetHiddenSingleton(implementation);
            }
        }

        public KeyedByTypeCollection<IServiceBehavior> Behaviors
        {
            get
            {
                return this.behaviors;
            }
        }

        public string ConfigurationName
        {
            get
            {
                return this.configurationName;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.configurationName = value;
            }
        }

        public ServiceEndpointCollection Endpoints
        {
            get
            {
                return this.endpoints;
            }
        }

        public string Name
        {
            get
            {
                if (this.serviceName != null)
                {
                    return this.serviceName.EncodedName;
                }
                if (this.ServiceType != null)
                {
                    return NamingHelper.XmlName(this.ServiceType.Name);
                }
                return "service";
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.serviceName = null;
                }
                else
                {
                    this.serviceName = new XmlName(value, true);
                }
            }
        }

        public string Namespace
        {
            get
            {
                return this.serviceNamespace;
            }
            set
            {
                this.serviceNamespace = value;
            }
        }

        public Type ServiceType
        {
            get
            {
                return this.serviceType;
            }
            set
            {
                this.serviceType = value;
            }
        }
    }
}

