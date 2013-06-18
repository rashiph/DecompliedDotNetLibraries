namespace System.Web.Services.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Services;
    using System.Web.Services.Description;
    using System.Web.Services.Discovery;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;

    public sealed class WebServicesSection : ConfigurationSection
    {
        private static object classSyncObject;
        private readonly ConfigurationProperty conformanceWarnings = new ConfigurationProperty("conformanceWarnings", typeof(WsiProfilesElementCollection), null, ConfigurationPropertyOptions.None);
        private Type[] defaultFormatTypes = new Type[] { 
            typeof(HttpAddressBinding), typeof(HttpBinding), typeof(HttpOperationBinding), typeof(HttpUrlEncodedBinding), typeof(HttpUrlReplacementBinding), typeof(MimeContentBinding), typeof(MimeXmlBinding), typeof(MimeMultipartRelatedBinding), typeof(MimeTextBinding), typeof(System.Web.Services.Description.SoapBinding), typeof(SoapOperationBinding), typeof(SoapBodyBinding), typeof(SoapFaultBinding), typeof(SoapHeaderBinding), typeof(SoapAddressBinding), typeof(Soap12Binding), 
            typeof(Soap12OperationBinding), typeof(Soap12BodyBinding), typeof(Soap12FaultBinding), typeof(Soap12HeaderBinding), typeof(Soap12AddressBinding)
         };
        private readonly ConfigurationProperty diagnostics = new ConfigurationProperty("diagnostics", typeof(DiagnosticsElement), null, ConfigurationPropertyOptions.None);
        private XmlSerializer discoveryDocumentSerializer;
        private Type[] discoveryReferenceTypes = new Type[] { typeof(DiscoveryDocumentReference), typeof(ContractReference), typeof(SchemaReference), typeof(System.Web.Services.Discovery.SoapBinding) };
        private WebServiceProtocols enabledProtocols;
        private Type[] mimeImporterTypes = new Type[] { typeof(MimeXmlImporter), typeof(MimeFormImporter), typeof(MimeTextImporter) };
        private Type[] mimeReflectorTypes = new Type[] { typeof(MimeXmlReflector), typeof(MimeFormReflector) };
        private Type[] parameterReaderTypes = new Type[] { typeof(UrlParameterReader), typeof(HtmlFormParameterReader) };
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private Type[] protocolImporterTypes = new Type[0];
        private Type[] protocolReflectorTypes = new Type[0];
        private readonly ConfigurationProperty protocols = new ConfigurationProperty("protocols", typeof(ProtocolElementCollection), null, ConfigurationPropertyOptions.None);
        private Type[] returnWriterTypes = new Type[] { typeof(XmlReturnWriter) };
        private const string SectionName = "system.web/webServices";
        private ServerProtocolFactory[] serverProtocolFactories;
        private readonly ConfigurationProperty serviceDescriptionFormatExtensionTypes = new ConfigurationProperty("serviceDescriptionFormatExtensionTypes", typeof(TypeElementCollection), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty soapEnvelopeProcessing = new ConfigurationProperty("soapEnvelopeProcessing", typeof(SoapEnvelopeProcessingElement), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty soapExtensionImporterTypes = new ConfigurationProperty("soapExtensionImporterTypes", typeof(TypeElementCollection), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty soapExtensionReflectorTypes = new ConfigurationProperty("soapExtensionReflectorTypes", typeof(TypeElementCollection), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty soapExtensionTypes = new ConfigurationProperty("soapExtensionTypes", typeof(SoapExtensionTypeElementCollection), null, ConfigurationPropertyOptions.None);
        private Type soapServerProtocolFactory;
        private readonly ConfigurationProperty soapServerProtocolFactoryType = new ConfigurationProperty("soapServerProtocolFactory", typeof(TypeElement), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty soapTransportImporterTypes = new ConfigurationProperty("soapTransportImporterTypes", typeof(TypeElementCollection), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty wsdlHelpGenerator = new ConfigurationProperty("wsdlHelpGenerator", typeof(WsdlHelpGeneratorElement), null, ConfigurationPropertyOptions.None);

        public WebServicesSection()
        {
            this.properties.Add(this.conformanceWarnings);
            this.properties.Add(this.protocols);
            this.properties.Add(this.serviceDescriptionFormatExtensionTypes);
            this.properties.Add(this.soapEnvelopeProcessing);
            this.properties.Add(this.soapExtensionImporterTypes);
            this.properties.Add(this.soapExtensionReflectorTypes);
            this.properties.Add(this.soapExtensionTypes);
            this.properties.Add(this.soapTransportImporterTypes);
            this.properties.Add(this.wsdlHelpGenerator);
            this.properties.Add(this.soapServerProtocolFactoryType);
            this.properties.Add(this.diagnostics);
        }

        internal Type[] GetAllFormatExtensionTypes()
        {
            if (this.ServiceDescriptionFormatExtensionTypes.Count == 0)
            {
                return this.defaultFormatTypes;
            }
            Type[] destinationArray = new Type[this.defaultFormatTypes.Length + this.ServiceDescriptionFormatExtensionTypes.Count];
            Array.Copy(this.defaultFormatTypes, destinationArray, this.defaultFormatTypes.Length);
            for (int i = 0; i < this.ServiceDescriptionFormatExtensionTypes.Count; i++)
            {
                destinationArray[i + this.defaultFormatTypes.Length] = this.ServiceDescriptionFormatExtensionTypes[i].Type;
            }
            return destinationArray;
        }

        [MethodImpl(MethodImplOptions.NoInlining), ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
        private static WebServicesSection GetConfigFromHttpContext()
        {
            HttpContext current = HttpContext.Current;
            if (current != null)
            {
                return (WebServicesSection) current.GetSection("system.web/webServices");
            }
            return null;
        }

        private static XmlFormatExtensionPointAttribute GetExtensionPointAttribute(Type type)
        {
            object[] customAttributes = type.GetCustomAttributes(typeof(XmlFormatExtensionPointAttribute), false);
            if (customAttributes.Length == 0)
            {
                throw new ArgumentException(Res.GetString("TheSyntaxOfTypeMayNotBeExtended1", new object[] { type.FullName }), "type");
            }
            return (XmlFormatExtensionPointAttribute) customAttributes[0];
        }

        [ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
        public static WebServicesSection GetSection(System.Configuration.Configuration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            return (WebServicesSection) config.GetSection("system.web/webServices");
        }

        protected override void InitializeDefault()
        {
            this.ConformanceWarnings.SetDefaults();
            this.Protocols.SetDefaults();
            if (Thread.GetDomain().GetData(".appDomain") != null)
            {
                this.WsdlHelpGenerator.SetDefaults();
            }
            this.SoapServerProtocolFactoryType.Type = typeof(System.Web.Services.Protocols.SoapServerProtocolFactory);
        }

        internal static void LoadXmlFormatExtensions(Type[] extensionTypes, XmlAttributeOverrides overrides, XmlSerializerNamespaces namespaces)
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add(typeof(ServiceDescription), new XmlAttributes());
            hashtable.Add(typeof(Import), new XmlAttributes());
            hashtable.Add(typeof(Port), new XmlAttributes());
            hashtable.Add(typeof(Service), new XmlAttributes());
            hashtable.Add(typeof(FaultBinding), new XmlAttributes());
            hashtable.Add(typeof(InputBinding), new XmlAttributes());
            hashtable.Add(typeof(OutputBinding), new XmlAttributes());
            hashtable.Add(typeof(OperationBinding), new XmlAttributes());
            hashtable.Add(typeof(Binding), new XmlAttributes());
            hashtable.Add(typeof(OperationFault), new XmlAttributes());
            hashtable.Add(typeof(OperationInput), new XmlAttributes());
            hashtable.Add(typeof(OperationOutput), new XmlAttributes());
            hashtable.Add(typeof(Operation), new XmlAttributes());
            hashtable.Add(typeof(PortType), new XmlAttributes());
            hashtable.Add(typeof(Message), new XmlAttributes());
            hashtable.Add(typeof(MessagePart), new XmlAttributes());
            hashtable.Add(typeof(Types), new XmlAttributes());
            Hashtable hashtable2 = new Hashtable();
            foreach (Type type in extensionTypes)
            {
                if (hashtable2[type] == null)
                {
                    hashtable2.Add(type, type);
                    object[] customAttributes = type.GetCustomAttributes(typeof(XmlFormatExtensionAttribute), false);
                    if (customAttributes.Length == 0)
                    {
                        throw new ArgumentException(Res.GetString("RequiredXmlFormatExtensionAttributeIsMissing1", new object[] { type.FullName }), "extensionTypes");
                    }
                    XmlFormatExtensionAttribute attribute = (XmlFormatExtensionAttribute) customAttributes[0];
                    foreach (Type type2 in attribute.ExtensionPoints)
                    {
                        XmlAttributes attributes = (XmlAttributes) hashtable[type2];
                        if (attributes == null)
                        {
                            attributes = new XmlAttributes();
                            hashtable.Add(type2, attributes);
                        }
                        XmlElementAttribute attribute2 = new XmlElementAttribute(attribute.ElementName, type) {
                            Namespace = attribute.Namespace
                        };
                        attributes.XmlElements.Add(attribute2);
                    }
                    customAttributes = type.GetCustomAttributes(typeof(XmlFormatExtensionPrefixAttribute), false);
                    string[] array = new string[customAttributes.Length];
                    Hashtable hashtable3 = new Hashtable();
                    for (int i = 0; i < customAttributes.Length; i++)
                    {
                        XmlFormatExtensionPrefixAttribute attribute3 = (XmlFormatExtensionPrefixAttribute) customAttributes[i];
                        array[i] = attribute3.Prefix;
                        hashtable3.Add(attribute3.Prefix, attribute3.Namespace);
                    }
                    Array.Sort(array, System.InvariantComparer.Default);
                    for (int j = 0; j < array.Length; j++)
                    {
                        namespaces.Add(array[j], (string) hashtable3[array[j]]);
                    }
                }
            }
            foreach (Type type3 in hashtable.Keys)
            {
                XmlFormatExtensionPointAttribute extensionPointAttribute = GetExtensionPointAttribute(type3);
                XmlAttributes attributes2 = (XmlAttributes) hashtable[type3];
                if (extensionPointAttribute.AllowElements)
                {
                    attributes2.XmlAnyElements.Add(new XmlAnyElementAttribute());
                }
                overrides.Add(type3, extensionPointAttribute.MemberName, attributes2);
            }
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            if (parentElement != null)
            {
                WebServicesSection section = (WebServicesSection) parentElement;
                this.discoveryDocumentSerializer = section.discoveryDocumentSerializer;
                this.serverProtocolFactories = section.serverProtocolFactories;
            }
            base.Reset(parentElement);
        }

        private void TurnOnGetAndPost()
        {
            bool flag = (this.EnabledProtocols & WebServiceProtocols.HttpPost) == WebServiceProtocols.Unknown;
            bool flag2 = (this.EnabledProtocols & WebServiceProtocols.HttpGet) == WebServiceProtocols.Unknown;
            if (flag2 || flag)
            {
                ArrayList list = new ArrayList(this.ProtocolImporterTypes);
                ArrayList list2 = new ArrayList(this.ProtocolReflectorTypes);
                if (flag)
                {
                    list.Add(typeof(HttpPostProtocolImporter));
                    list2.Add(typeof(HttpPostProtocolReflector));
                }
                if (flag2)
                {
                    list.Add(typeof(HttpGetProtocolImporter));
                    list2.Add(typeof(HttpGetProtocolReflector));
                }
                this.ProtocolImporterTypes = (Type[]) list.ToArray(typeof(Type));
                this.ProtocolReflectorTypes = (Type[]) list2.ToArray(typeof(Type));
                this.enabledProtocols |= WebServiceProtocols.HttpPost | WebServiceProtocols.HttpGet;
            }
        }

        private static object ClassSyncObject
        {
            get
            {
                if (classSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref classSyncObject, obj2, null);
                }
                return classSyncObject;
            }
        }

        [ConfigurationProperty("conformanceWarnings")]
        public WsiProfilesElementCollection ConformanceWarnings
        {
            get
            {
                return (WsiProfilesElementCollection) base[this.conformanceWarnings];
            }
        }

        public static WebServicesSection Current
        {
            get
            {
                WebServicesSection configFromHttpContext = null;
                if (Thread.GetDomain().GetData(".appDomain") != null)
                {
                    configFromHttpContext = GetConfigFromHttpContext();
                }
                if (configFromHttpContext == null)
                {
                    configFromHttpContext = (WebServicesSection) System.Configuration.PrivilegedConfigurationManager.GetSection("system.web/webServices");
                }
                return configFromHttpContext;
            }
        }

        public DiagnosticsElement Diagnostics
        {
            get
            {
                return (DiagnosticsElement) base[this.diagnostics];
            }
            set
            {
                base[this.diagnostics] = value;
            }
        }

        internal XmlSerializer DiscoveryDocumentSerializer
        {
            get
            {
                if (this.discoveryDocumentSerializer == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (this.discoveryDocumentSerializer == null)
                        {
                            XmlAttributeOverrides overrides = new XmlAttributeOverrides();
                            XmlAttributes attributes = new XmlAttributes();
                            foreach (Type type in this.DiscoveryReferenceTypes)
                            {
                                object[] customAttributes = type.GetCustomAttributes(typeof(XmlRootAttribute), false);
                                if (customAttributes.Length == 0)
                                {
                                    throw new InvalidOperationException(Res.GetString("WebMissingCustomAttribute", new object[] { type.FullName, "XmlRoot" }));
                                }
                                string elementName = ((XmlRootAttribute) customAttributes[0]).ElementName;
                                string str2 = ((XmlRootAttribute) customAttributes[0]).Namespace;
                                XmlElementAttribute attribute = new XmlElementAttribute(elementName, type) {
                                    Namespace = str2
                                };
                                attributes.XmlElements.Add(attribute);
                            }
                            overrides.Add(typeof(DiscoveryDocument), "References", attributes);
                            this.discoveryDocumentSerializer = new System.Web.Services.Discovery.DiscoveryDocumentSerializer();
                        }
                    }
                }
                return this.discoveryDocumentSerializer;
            }
        }

        internal Type[] DiscoveryReferenceTypes
        {
            get
            {
                return this.discoveryReferenceTypes;
            }
        }

        internal WsiProfiles EnabledConformanceWarnings
        {
            get
            {
                WsiProfiles none = WsiProfiles.None;
                foreach (WsiProfilesElement element in this.ConformanceWarnings)
                {
                    none |= element.Name;
                }
                return none;
            }
        }

        public WebServiceProtocols EnabledProtocols
        {
            get
            {
                if (this.enabledProtocols == WebServiceProtocols.Unknown)
                {
                    lock (ClassSyncObject)
                    {
                        if (this.enabledProtocols == WebServiceProtocols.Unknown)
                        {
                            WebServiceProtocols unknown = WebServiceProtocols.Unknown;
                            foreach (ProtocolElement element in this.Protocols)
                            {
                                unknown |= element.Name;
                            }
                            this.enabledProtocols = unknown;
                        }
                    }
                }
                return this.enabledProtocols;
            }
        }

        internal Type[] MimeImporterTypes
        {
            get
            {
                return this.mimeImporterTypes;
            }
        }

        internal Type[] MimeReflectorTypes
        {
            get
            {
                return this.mimeReflectorTypes;
            }
        }

        internal Type[] ParameterReaderTypes
        {
            get
            {
                return this.parameterReaderTypes;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.properties;
            }
        }

        internal Type[] ProtocolImporterTypes
        {
            get
            {
                if (this.protocolImporterTypes.Length == 0)
                {
                    lock (ClassSyncObject)
                    {
                        if (this.protocolImporterTypes.Length == 0)
                        {
                            WebServiceProtocols enabledProtocols = this.EnabledProtocols;
                            List<Type> list = new List<Type>();
                            if ((enabledProtocols & WebServiceProtocols.HttpSoap) != WebServiceProtocols.Unknown)
                            {
                                list.Add(typeof(SoapProtocolImporter));
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpSoap12) != WebServiceProtocols.Unknown)
                            {
                                list.Add(typeof(Soap12ProtocolImporter));
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpGet) != WebServiceProtocols.Unknown)
                            {
                                list.Add(typeof(HttpGetProtocolImporter));
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpPost) != WebServiceProtocols.Unknown)
                            {
                                list.Add(typeof(HttpPostProtocolImporter));
                            }
                            this.protocolImporterTypes = list.ToArray();
                        }
                    }
                }
                return this.protocolImporterTypes;
            }
            set
            {
                this.protocolImporterTypes = value;
            }
        }

        internal Type[] ProtocolReflectorTypes
        {
            get
            {
                if (this.protocolReflectorTypes.Length == 0)
                {
                    lock (ClassSyncObject)
                    {
                        if (this.protocolReflectorTypes.Length == 0)
                        {
                            WebServiceProtocols enabledProtocols = this.EnabledProtocols;
                            List<Type> list = new List<Type>();
                            if ((enabledProtocols & WebServiceProtocols.HttpSoap) != WebServiceProtocols.Unknown)
                            {
                                list.Add(typeof(SoapProtocolReflector));
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpSoap12) != WebServiceProtocols.Unknown)
                            {
                                list.Add(typeof(Soap12ProtocolReflector));
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpGet) != WebServiceProtocols.Unknown)
                            {
                                list.Add(typeof(HttpGetProtocolReflector));
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpPost) != WebServiceProtocols.Unknown)
                            {
                                list.Add(typeof(HttpPostProtocolReflector));
                            }
                            this.protocolReflectorTypes = list.ToArray();
                        }
                    }
                }
                return this.protocolReflectorTypes;
            }
            set
            {
                this.protocolReflectorTypes = value;
            }
        }

        [ConfigurationProperty("protocols")]
        public ProtocolElementCollection Protocols
        {
            get
            {
                return (ProtocolElementCollection) base[this.protocols];
            }
        }

        internal Type[] ReturnWriterTypes
        {
            get
            {
                return this.returnWriterTypes;
            }
        }

        internal ServerProtocolFactory[] ServerProtocolFactories
        {
            get
            {
                if (this.serverProtocolFactories == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (this.serverProtocolFactories == null)
                        {
                            WebServiceProtocols enabledProtocols = this.EnabledProtocols;
                            List<ServerProtocolFactory> list = new List<ServerProtocolFactory>();
                            if ((enabledProtocols & WebServiceProtocols.AnyHttpSoap) != WebServiceProtocols.Unknown)
                            {
                                list.Add((ServerProtocolFactory) Activator.CreateInstance(this.SoapServerProtocolFactory));
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpPost) != WebServiceProtocols.Unknown)
                            {
                                list.Add(new HttpPostServerProtocolFactory());
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpPostLocalhost) != WebServiceProtocols.Unknown)
                            {
                                list.Add(new HttpPostLocalhostServerProtocolFactory());
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpGet) != WebServiceProtocols.Unknown)
                            {
                                list.Add(new HttpGetServerProtocolFactory());
                            }
                            if ((enabledProtocols & WebServiceProtocols.Documentation) != WebServiceProtocols.Unknown)
                            {
                                list.Add(new DiscoveryServerProtocolFactory());
                                list.Add(new DocumentationServerProtocolFactory());
                            }
                            this.serverProtocolFactories = list.ToArray();
                        }
                    }
                }
                return this.serverProtocolFactories;
            }
        }

        internal bool ServiceDescriptionExtended
        {
            get
            {
                return (this.ServiceDescriptionFormatExtensionTypes.Count > 0);
            }
        }

        [ConfigurationProperty("serviceDescriptionFormatExtensionTypes")]
        public TypeElementCollection ServiceDescriptionFormatExtensionTypes
        {
            get
            {
                return (TypeElementCollection) base[this.serviceDescriptionFormatExtensionTypes];
            }
        }

        [ConfigurationProperty("soapEnvelopeProcessing")]
        public SoapEnvelopeProcessingElement SoapEnvelopeProcessing
        {
            get
            {
                return (SoapEnvelopeProcessingElement) base[this.soapEnvelopeProcessing];
            }
            set
            {
                base[this.soapEnvelopeProcessing] = value;
            }
        }

        [ConfigurationProperty("soapExtensionImporterTypes")]
        public TypeElementCollection SoapExtensionImporterTypes
        {
            get
            {
                return (TypeElementCollection) base[this.soapExtensionImporterTypes];
            }
        }

        [ConfigurationProperty("soapExtensionReflectorTypes")]
        public TypeElementCollection SoapExtensionReflectorTypes
        {
            get
            {
                return (TypeElementCollection) base[this.soapExtensionReflectorTypes];
            }
        }

        [ConfigurationProperty("soapExtensionTypes")]
        public SoapExtensionTypeElementCollection SoapExtensionTypes
        {
            get
            {
                return (SoapExtensionTypeElementCollection) base[this.soapExtensionTypes];
            }
        }

        internal Type SoapServerProtocolFactory
        {
            get
            {
                if (this.soapServerProtocolFactory == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (this.soapServerProtocolFactory == null)
                        {
                            this.soapServerProtocolFactory = this.SoapServerProtocolFactoryType.Type;
                        }
                    }
                }
                return this.soapServerProtocolFactory;
            }
        }

        [ConfigurationProperty("soapServerProtocolFactory")]
        public TypeElement SoapServerProtocolFactoryType
        {
            get
            {
                return (TypeElement) base[this.soapServerProtocolFactoryType];
            }
        }

        internal Type[] SoapTransportImporters
        {
            get
            {
                Type[] typeArray = new Type[1 + this.SoapTransportImporterTypes.Count];
                typeArray[0] = typeof(SoapHttpTransportImporter);
                for (int i = 0; i < this.SoapTransportImporterTypes.Count; i++)
                {
                    typeArray[i + 1] = this.SoapTransportImporterTypes[i].Type;
                }
                return typeArray;
            }
        }

        [ConfigurationProperty("soapTransportImporterTypes")]
        public TypeElementCollection SoapTransportImporterTypes
        {
            get
            {
                return (TypeElementCollection) base[this.soapTransportImporterTypes];
            }
        }

        [ConfigurationProperty("wsdlHelpGenerator")]
        public WsdlHelpGeneratorElement WsdlHelpGenerator
        {
            get
            {
                return (WsdlHelpGeneratorElement) base[this.wsdlHelpGenerator];
            }
        }
    }
}

