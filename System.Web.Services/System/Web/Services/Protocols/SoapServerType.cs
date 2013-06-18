namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Diagnostics;
    using System.Xml;
    using System.Xml.Serialization;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class SoapServerType : ServerType
    {
        private Hashtable duplicateMethods;
        internal object[] HighPriExtensionInitializers;
        internal SoapReflectedExtension[] HighPriExtensions;
        internal object[] LowPriExtensionInitializers;
        internal SoapReflectedExtension[] LowPriExtensions;
        private Hashtable methods;
        internal WebServiceProtocols protocolsSupported;
        internal bool routingOnSoapAction;
        internal bool serviceDefaultIsEncoded;
        internal string serviceNamespace;

        public SoapServerType(Type type, WebServiceProtocols protocolsSupported) : base(type)
        {
            this.methods = new Hashtable();
            this.duplicateMethods = new Hashtable();
            this.protocolsSupported = protocolsSupported;
            bool flag = (protocolsSupported & WebServiceProtocols.HttpSoap) != WebServiceProtocols.Unknown;
            LogicalMethodInfo[] methods = WebMethodReflector.GetMethods(type);
            ArrayList list = new ArrayList();
            WebServiceAttribute attribute = WebServiceReflector.GetAttribute(type);
            object soapServiceAttribute = SoapReflector.GetSoapServiceAttribute(type);
            this.routingOnSoapAction = SoapReflector.GetSoapServiceRoutingStyle(soapServiceAttribute) == SoapServiceRoutingStyle.SoapAction;
            this.serviceNamespace = attribute.Namespace;
            this.serviceDefaultIsEncoded = SoapReflector.ServiceDefaultIsEncoded(type);
            SoapReflectionImporter importer = SoapReflector.CreateSoapImporter(this.serviceNamespace, this.serviceDefaultIsEncoded);
            XmlReflectionImporter importer2 = SoapReflector.CreateXmlImporter(this.serviceNamespace, this.serviceDefaultIsEncoded);
            SoapReflector.IncludeTypes(methods, importer);
            WebMethodReflector.IncludeTypes(methods, importer2);
            SoapReflectedMethod[] methodArray = new SoapReflectedMethod[methods.Length];
            SoapExtensionTypeElementCollection soapExtensionTypes = WebServicesSection.Current.SoapExtensionTypes;
            ArrayList list2 = new ArrayList();
            ArrayList list3 = new ArrayList();
            for (int i = 0; i < soapExtensionTypes.Count; i++)
            {
                SoapExtensionTypeElement element = soapExtensionTypes[i];
                if (element != null)
                {
                    SoapReflectedExtension extension = new SoapReflectedExtension(element.Type, null, element.Priority);
                    if (element.Group == PriorityGroup.High)
                    {
                        list2.Add(extension);
                    }
                    else
                    {
                        list3.Add(extension);
                    }
                }
            }
            this.HighPriExtensions = (SoapReflectedExtension[]) list2.ToArray(typeof(SoapReflectedExtension));
            this.LowPriExtensions = (SoapReflectedExtension[]) list3.ToArray(typeof(SoapReflectedExtension));
            Array.Sort<SoapReflectedExtension>(this.HighPriExtensions);
            Array.Sort<SoapReflectedExtension>(this.LowPriExtensions);
            this.HighPriExtensionInitializers = SoapReflectedExtension.GetInitializers(type, this.HighPriExtensions);
            this.LowPriExtensionInitializers = SoapReflectedExtension.GetInitializers(type, this.LowPriExtensions);
            for (int j = 0; j < methods.Length; j++)
            {
                LogicalMethodInfo methodInfo = methods[j];
                SoapReflectedMethod method = SoapReflector.ReflectMethod(methodInfo, false, importer2, importer, attribute.Namespace);
                list.Add(method.requestMappings);
                if (method.responseMappings != null)
                {
                    list.Add(method.responseMappings);
                }
                list.Add(method.inHeaderMappings);
                if (method.outHeaderMappings != null)
                {
                    list.Add(method.outHeaderMappings);
                }
                methodArray[j] = method;
            }
            XmlMapping[] mappings = (XmlMapping[]) list.ToArray(typeof(XmlMapping));
            TraceMethod caller = Tracing.On ? new TraceMethod(this, ".ctor", new object[] { type, protocolsSupported }) : null;
            if (Tracing.On)
            {
                Tracing.Enter(Tracing.TraceId("TraceCreateSerializer"), caller, new TraceMethod(typeof(XmlSerializer), "FromMappings", new object[] { mappings, base.Evidence }));
            }
            XmlSerializer[] serializerArray = null;
            if (AppDomain.CurrentDomain.IsHomogenous)
            {
                serializerArray = XmlSerializer.FromMappings(mappings);
            }
            else
            {
                serializerArray = XmlSerializer.FromMappings(mappings, base.Evidence);
            }
            if (Tracing.On)
            {
                Tracing.Exit(Tracing.TraceId("TraceCreateSerializer"), caller);
            }
            int num3 = 0;
            for (int k = 0; k < methodArray.Length; k++)
            {
                SoapServerMethod method3 = new SoapServerMethod();
                SoapReflectedMethod method4 = methodArray[k];
                method3.parameterSerializer = serializerArray[num3++];
                if (method4.responseMappings != null)
                {
                    method3.returnSerializer = serializerArray[num3++];
                }
                method3.inHeaderSerializer = serializerArray[num3++];
                if (method4.outHeaderMappings != null)
                {
                    method3.outHeaderSerializer = serializerArray[num3++];
                }
                method3.methodInfo = method4.methodInfo;
                method3.action = method4.action;
                method3.extensions = method4.extensions;
                method3.extensionInitializers = SoapReflectedExtension.GetInitializers(method3.methodInfo, method4.extensions);
                method3.oneWay = method4.oneWay;
                method3.rpc = method4.rpc;
                method3.use = method4.use;
                method3.paramStyle = method4.paramStyle;
                method3.wsiClaims = (method4.binding == null) ? WsiProfiles.None : method4.binding.ConformsTo;
                ArrayList list4 = new ArrayList();
                ArrayList list5 = new ArrayList();
                for (int m = 0; m < method4.headers.Length; m++)
                {
                    SoapHeaderMapping mapping = new SoapHeaderMapping();
                    SoapReflectedHeader header = method4.headers[m];
                    mapping.memberInfo = header.memberInfo;
                    mapping.repeats = header.repeats;
                    mapping.custom = header.custom;
                    mapping.direction = header.direction;
                    mapping.headerType = header.headerType;
                    if (mapping.direction == SoapHeaderDirection.In)
                    {
                        list4.Add(mapping);
                    }
                    else if (mapping.direction == SoapHeaderDirection.Out)
                    {
                        list5.Add(mapping);
                    }
                    else
                    {
                        list4.Add(mapping);
                        list5.Add(mapping);
                    }
                }
                method3.inHeaderMappings = (SoapHeaderMapping[]) list4.ToArray(typeof(SoapHeaderMapping));
                if (method3.outHeaderSerializer != null)
                {
                    method3.outHeaderMappings = (SoapHeaderMapping[]) list5.ToArray(typeof(SoapHeaderMapping));
                }
                if ((flag && !this.routingOnSoapAction) && method4.requestElementName.IsEmpty)
                {
                    throw new SoapException(System.Web.Services.Res.GetString("TheMethodDoesNotHaveARequestElementEither1", new object[] { method3.methodInfo.Name }), new XmlQualifiedName("Client", "http://schemas.xmlsoap.org/soap/envelope/"));
                }
                if (this.methods[method4.action] == null)
                {
                    this.methods[method4.action] = method3;
                }
                else
                {
                    if (flag && this.routingOnSoapAction)
                    {
                        SoapServerMethod method5 = (SoapServerMethod) this.methods[method4.action];
                        throw new SoapException(System.Web.Services.Res.GetString("TheMethodsAndUseTheSameSoapActionWhenTheService3", new object[] { method3.methodInfo.Name, method5.methodInfo.Name, method4.action }), new XmlQualifiedName("Client", "http://schemas.xmlsoap.org/soap/envelope/"));
                    }
                    this.duplicateMethods[method4.action] = method3;
                }
                if (this.methods[method4.requestElementName] == null)
                {
                    this.methods[method4.requestElementName] = method3;
                }
                else
                {
                    if (flag && !this.routingOnSoapAction)
                    {
                        SoapServerMethod method6 = (SoapServerMethod) this.methods[method4.requestElementName];
                        throw new SoapException(System.Web.Services.Res.GetString("TheMethodsAndUseTheSameRequestElementXmlns4", new object[] { method3.methodInfo.Name, method6.methodInfo.Name, method4.requestElementName.Name, method4.requestElementName.Namespace }), new XmlQualifiedName("Client", "http://schemas.xmlsoap.org/soap/envelope/"));
                    }
                    this.duplicateMethods[method4.requestElementName] = method3;
                }
            }
        }

        public SoapServerMethod GetDuplicateMethod(object key)
        {
            return (SoapServerMethod) this.duplicateMethods[key];
        }

        public SoapServerMethod GetMethod(object key)
        {
            return (SoapServerMethod) this.methods[key];
        }

        public bool ServiceDefaultIsEncoded
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serviceDefaultIsEncoded;
            }
        }

        public string ServiceNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serviceNamespace;
            }
        }

        public bool ServiceRoutingOnSoapAction
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.routingOnSoapAction;
            }
        }
    }
}

