namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Diagnostics;
    using System.Xml.Serialization;

    internal class SoapClientType
    {
        private WebServiceBindingAttribute binding;
        internal object[] HighPriExtensionInitializers;
        internal SoapReflectedExtension[] HighPriExtensions;
        internal object[] LowPriExtensionInitializers;
        internal SoapReflectedExtension[] LowPriExtensions;
        private Hashtable methods = new Hashtable();
        internal bool serviceDefaultIsEncoded;
        internal string serviceNamespace;

        internal SoapClientType(Type type)
        {
            this.binding = WebServiceBindingReflector.GetAttribute(type);
            if (this.binding == null)
            {
                throw new InvalidOperationException(Res.GetString("WebClientBindingAttributeRequired"));
            }
            this.serviceNamespace = this.binding.Namespace;
            this.serviceDefaultIsEncoded = SoapReflector.ServiceDefaultIsEncoded(type);
            ArrayList soapMethodList = new ArrayList();
            ArrayList mappings = new ArrayList();
            GenerateXmlMappings(type, soapMethodList, this.serviceNamespace, this.serviceDefaultIsEncoded, mappings);
            XmlMapping[] mappingArray = (XmlMapping[]) mappings.ToArray(typeof(XmlMapping));
            TraceMethod caller = Tracing.On ? new TraceMethod(this, ".ctor", new object[] { type }) : null;
            if (Tracing.On)
            {
                Tracing.Enter(Tracing.TraceId("TraceCreateSerializer"), caller, new TraceMethod(typeof(XmlSerializer), "FromMappings", new object[] { mappingArray, type }));
            }
            XmlSerializer[] serializerArray = XmlSerializer.FromMappings(mappingArray, type);
            if (Tracing.On)
            {
                Tracing.Exit(Tracing.TraceId("TraceCreateSerializer"), caller);
            }
            SoapExtensionTypeElementCollection soapExtensionTypes = WebServicesSection.Current.SoapExtensionTypes;
            ArrayList list3 = new ArrayList();
            ArrayList list4 = new ArrayList();
            for (int i = 0; i < soapExtensionTypes.Count; i++)
            {
                SoapExtensionTypeElement element1 = soapExtensionTypes[i];
                SoapReflectedExtension extension = new SoapReflectedExtension(soapExtensionTypes[i].Type, null, soapExtensionTypes[i].Priority);
                if (soapExtensionTypes[i].Group == PriorityGroup.High)
                {
                    list3.Add(extension);
                }
                else
                {
                    list4.Add(extension);
                }
            }
            this.HighPriExtensions = (SoapReflectedExtension[]) list3.ToArray(typeof(SoapReflectedExtension));
            this.LowPriExtensions = (SoapReflectedExtension[]) list4.ToArray(typeof(SoapReflectedExtension));
            Array.Sort<SoapReflectedExtension>(this.HighPriExtensions);
            Array.Sort<SoapReflectedExtension>(this.LowPriExtensions);
            this.HighPriExtensionInitializers = SoapReflectedExtension.GetInitializers(type, this.HighPriExtensions);
            this.LowPriExtensionInitializers = SoapReflectedExtension.GetInitializers(type, this.LowPriExtensions);
            int num2 = 0;
            for (int j = 0; j < soapMethodList.Count; j++)
            {
                SoapReflectedMethod method2 = (SoapReflectedMethod) soapMethodList[j];
                SoapClientMethod method3 = new SoapClientMethod {
                    parameterSerializer = serializerArray[num2++]
                };
                if (method2.responseMappings != null)
                {
                    method3.returnSerializer = serializerArray[num2++];
                }
                method3.inHeaderSerializer = serializerArray[num2++];
                if (method2.outHeaderMappings != null)
                {
                    method3.outHeaderSerializer = serializerArray[num2++];
                }
                method3.action = method2.action;
                method3.oneWay = method2.oneWay;
                method3.rpc = method2.rpc;
                method3.use = method2.use;
                method3.paramStyle = method2.paramStyle;
                method3.methodInfo = method2.methodInfo;
                method3.extensions = method2.extensions;
                method3.extensionInitializers = SoapReflectedExtension.GetInitializers(method3.methodInfo, method2.extensions);
                ArrayList list5 = new ArrayList();
                ArrayList list6 = new ArrayList();
                for (int k = 0; k < method2.headers.Length; k++)
                {
                    SoapHeaderMapping mapping = new SoapHeaderMapping();
                    SoapReflectedHeader header = method2.headers[k];
                    mapping.memberInfo = header.memberInfo;
                    mapping.repeats = header.repeats;
                    mapping.custom = header.custom;
                    mapping.direction = header.direction;
                    mapping.headerType = header.headerType;
                    if ((mapping.direction & SoapHeaderDirection.In) != 0)
                    {
                        list5.Add(mapping);
                    }
                    if ((mapping.direction & (SoapHeaderDirection.Fault | SoapHeaderDirection.Out)) != 0)
                    {
                        list6.Add(mapping);
                    }
                }
                method3.inHeaderMappings = (SoapHeaderMapping[]) list5.ToArray(typeof(SoapHeaderMapping));
                if (method3.outHeaderSerializer != null)
                {
                    method3.outHeaderMappings = (SoapHeaderMapping[]) list6.ToArray(typeof(SoapHeaderMapping));
                }
                this.methods.Add(method2.name, method3);
            }
        }

        internal static void GenerateXmlMappings(Type type, ArrayList soapMethodList, string serviceNamespace, bool serviceDefaultIsEncoded, ArrayList mappings)
        {
            LogicalMethodInfo[] methods = LogicalMethodInfo.Create(type.GetMethods(BindingFlags.Public | BindingFlags.Instance), LogicalMethodTypes.Sync);
            SoapReflectionImporter importer = SoapReflector.CreateSoapImporter(serviceNamespace, serviceDefaultIsEncoded);
            XmlReflectionImporter importer2 = SoapReflector.CreateXmlImporter(serviceNamespace, serviceDefaultIsEncoded);
            WebMethodReflector.IncludeTypes(methods, importer2);
            SoapReflector.IncludeTypes(methods, importer);
            for (int i = 0; i < methods.Length; i++)
            {
                LogicalMethodInfo methodInfo = methods[i];
                SoapReflectedMethod method = SoapReflector.ReflectMethod(methodInfo, true, importer2, importer, serviceNamespace);
                if (method != null)
                {
                    soapMethodList.Add(method);
                    mappings.Add(method.requestMappings);
                    if (method.responseMappings != null)
                    {
                        mappings.Add(method.responseMappings);
                    }
                    mappings.Add(method.inHeaderMappings);
                    if (method.outHeaderMappings != null)
                    {
                        mappings.Add(method.outHeaderMappings);
                    }
                }
            }
        }

        internal SoapClientMethod GetMethod(string name)
        {
            return (SoapClientMethod) this.methods[name];
        }

        internal WebServiceBindingAttribute Binding
        {
            get
            {
                return this.binding;
            }
        }
    }
}

