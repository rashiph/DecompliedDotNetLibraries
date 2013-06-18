namespace System.Web.Services.Description
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class ServiceDescriptionReflector
    {
        private System.Web.Services.Description.ServiceDescription description;
        private ServiceDescriptionCollection descriptions = new ServiceDescriptionCollection();
        private ServiceDescriptionCollection descriptionsWithPost;
        private XmlSchemaExporter exporter;
        private XmlReflectionImporter importer;
        private LogicalMethodInfo[] methods;
        private Hashtable reflectionContext;
        private ProtocolReflector[] reflectors;
        private ProtocolReflector[] reflectorsWithPost;
        private XmlSchemas schemas = new XmlSchemas();
        private XmlSchemas schemasWithPost;
        private System.Web.Services.Description.Service service;
        private WebServiceAttribute serviceAttr;
        private Type serviceType;
        private string serviceUrl;

        public ServiceDescriptionReflector()
        {
            Type[] protocolReflectorTypes = WebServicesSection.Current.ProtocolReflectorTypes;
            this.reflectors = new ProtocolReflector[protocolReflectorTypes.Length];
            for (int i = 0; i < this.reflectors.Length; i++)
            {
                ProtocolReflector reflector = (ProtocolReflector) Activator.CreateInstance(protocolReflectorTypes[i]);
                reflector.Initialize(this);
                this.reflectors[i] = reflector;
            }
            WebServiceProtocols enabledProtocols = WebServicesSection.Current.EnabledProtocols;
            if (((enabledProtocols & WebServiceProtocols.HttpPost) == WebServiceProtocols.Unknown) && ((enabledProtocols & WebServiceProtocols.HttpPostLocalhost) != WebServiceProtocols.Unknown))
            {
                this.reflectorsWithPost = new ProtocolReflector[this.reflectors.Length + 1];
                for (int j = 0; j < (this.reflectorsWithPost.Length - 1); j++)
                {
                    ProtocolReflector reflector2 = (ProtocolReflector) Activator.CreateInstance(protocolReflectorTypes[j]);
                    reflector2.Initialize(this);
                    this.reflectorsWithPost[j] = reflector2;
                }
                ProtocolReflector reflector3 = new HttpPostProtocolReflector();
                reflector3.Initialize(this);
                this.reflectorsWithPost[this.reflectorsWithPost.Length - 1] = reflector3;
            }
        }

        private void CheckForDuplicateMethods(LogicalMethodInfo[] methods)
        {
            Hashtable hashtable = new Hashtable();
            foreach (LogicalMethodInfo info in methods)
            {
                string messageName = info.MethodAttribute.MessageName;
                if (messageName.Length == 0)
                {
                    messageName = info.Name;
                }
                string key = (info.Binding == null) ? messageName : (info.Binding.Name + "." + messageName);
                LogicalMethodInfo info2 = (LogicalMethodInfo) hashtable[key];
                if (info2 != null)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("BothAndUseTheMessageNameUseTheMessageName3", new object[] { info, info2, XmlConvert.EncodeLocalName(messageName) }));
                }
                hashtable.Add(key, info);
            }
        }

        public void Reflect(Type type, string url)
        {
            this.serviceType = type;
            this.serviceUrl = url;
            this.serviceAttr = WebServiceReflector.GetAttribute(type);
            this.methods = WebMethodReflector.GetMethods(type);
            this.CheckForDuplicateMethods(this.methods);
            this.descriptionsWithPost = this.descriptions;
            this.schemasWithPost = this.schemas;
            if (this.reflectorsWithPost != null)
            {
                this.ReflectInternal(this.reflectorsWithPost);
                this.descriptions = new ServiceDescriptionCollection();
                this.schemas = new XmlSchemas();
            }
            this.ReflectInternal(this.reflectors);
            if ((this.serviceAttr.Description != null) && (this.serviceAttr.Description.Length > 0))
            {
                this.ServiceDescription.Documentation = this.serviceAttr.Description;
            }
            this.ServiceDescription.Types.Schemas.Compile(null, false);
            if (this.ServiceDescriptions.Count > 1)
            {
                this.Schemas.Add(this.ServiceDescription.Types.Schemas);
                this.ServiceDescription.Types.Schemas.Clear();
            }
            else if (this.ServiceDescription.Types.Schemas.Count > 0)
            {
                XmlSchema[] array = new XmlSchema[this.ServiceDescription.Types.Schemas.Count];
                this.ServiceDescription.Types.Schemas.CopyTo(array, 0);
                foreach (XmlSchema schema in array)
                {
                    if (XmlSchemas.IsDataSet(schema))
                    {
                        this.ServiceDescription.Types.Schemas.Remove(schema);
                        this.Schemas.Add(schema);
                    }
                }
            }
        }

        private void ReflectInternal(ProtocolReflector[] reflectors)
        {
            this.description = new System.Web.Services.Description.ServiceDescription();
            this.description.TargetNamespace = this.serviceAttr.Namespace;
            this.ServiceDescriptions.Add(this.description);
            this.service = new System.Web.Services.Description.Service();
            string name = this.serviceAttr.Name;
            if ((name == null) || (name.Length == 0))
            {
                name = this.serviceType.Name;
            }
            this.service.Name = XmlConvert.EncodeLocalName(name);
            if ((this.serviceAttr.Description != null) && (this.serviceAttr.Description.Length > 0))
            {
                this.service.Documentation = this.serviceAttr.Description;
            }
            this.description.Services.Add(this.service);
            this.reflectionContext = new Hashtable();
            this.exporter = new XmlSchemaExporter(this.description.Types.Schemas);
            this.importer = SoapReflector.CreateXmlImporter(this.serviceAttr.Namespace, SoapReflector.ServiceDefaultIsEncoded(this.serviceType));
            WebMethodReflector.IncludeTypes(this.methods, this.importer);
            for (int i = 0; i < reflectors.Length; i++)
            {
                reflectors[i].Reflect();
            }
        }

        internal LogicalMethodInfo[] Methods
        {
            get
            {
                return this.methods;
            }
        }

        internal Hashtable ReflectionContext
        {
            get
            {
                if (this.reflectionContext == null)
                {
                    this.reflectionContext = new Hashtable();
                }
                return this.reflectionContext;
            }
        }

        internal XmlReflectionImporter ReflectionImporter
        {
            get
            {
                return this.importer;
            }
        }

        internal XmlSchemaExporter SchemaExporter
        {
            get
            {
                return this.exporter;
            }
        }

        public XmlSchemas Schemas
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.schemas;
            }
        }

        internal XmlSchemas SchemasWithPost
        {
            get
            {
                return this.schemasWithPost;
            }
        }

        internal System.Web.Services.Description.Service Service
        {
            get
            {
                return this.service;
            }
        }

        internal WebServiceAttribute ServiceAttribute
        {
            get
            {
                return this.serviceAttr;
            }
        }

        internal System.Web.Services.Description.ServiceDescription ServiceDescription
        {
            get
            {
                return this.description;
            }
        }

        public ServiceDescriptionCollection ServiceDescriptions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.descriptions;
            }
        }

        internal ServiceDescriptionCollection ServiceDescriptionsWithPost
        {
            get
            {
                return this.descriptionsWithPost;
            }
        }

        internal Type ServiceType
        {
            get
            {
                return this.serviceType;
            }
        }

        internal string ServiceUrl
        {
            get
            {
                return this.serviceUrl;
            }
        }
    }
}

