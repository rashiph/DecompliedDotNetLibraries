namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Web.Services;
    using System.Web.Services.Description;
    using System.Web.Services.Diagnostics;
    using System.Xml.Serialization;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class SoapServerMethod
    {
        internal string action;
        internal object[] extensionInitializers;
        internal SoapReflectedExtension[] extensions;
        internal SoapHeaderMapping[] inHeaderMappings;
        internal XmlSerializer inHeaderSerializer;
        internal LogicalMethodInfo methodInfo;
        internal bool oneWay;
        internal SoapHeaderMapping[] outHeaderMappings;
        internal XmlSerializer outHeaderSerializer;
        internal XmlSerializer parameterSerializer;
        internal SoapParameterStyle paramStyle;
        internal XmlSerializer returnSerializer;
        internal bool rpc;
        internal SoapBindingUse use;
        internal WsiProfiles wsiClaims;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SoapServerMethod()
        {
        }

        public SoapServerMethod(Type serverType, LogicalMethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
            string defaultNs = WebServiceReflector.GetAttribute(serverType).Namespace;
            bool serviceDefaultIsEncoded = SoapReflector.ServiceDefaultIsEncoded(serverType);
            SoapReflectionImporter importer = SoapReflector.CreateSoapImporter(defaultNs, serviceDefaultIsEncoded);
            XmlReflectionImporter importer2 = SoapReflector.CreateXmlImporter(defaultNs, serviceDefaultIsEncoded);
            SoapReflector.IncludeTypes(methodInfo, importer);
            WebMethodReflector.IncludeTypes(methodInfo, importer2);
            SoapReflectedMethod soapMethod = SoapReflector.ReflectMethod(methodInfo, false, importer2, importer, defaultNs);
            this.ImportReflectedMethod(soapMethod);
            this.ImportSerializers(soapMethod, this.GetServerTypeEvidence(serverType));
            this.ImportHeaderSerializers(soapMethod);
        }

        [SecurityPermission(SecurityAction.Assert, ControlEvidence=true)]
        private Evidence GetServerTypeEvidence(Type type)
        {
            return type.Assembly.Evidence;
        }

        private List<XmlMapping> GetXmlMappingsForMethod(SoapReflectedMethod soapMethod)
        {
            List<XmlMapping> list = new List<XmlMapping> {
                soapMethod.requestMappings
            };
            if (soapMethod.responseMappings != null)
            {
                list.Add(soapMethod.responseMappings);
            }
            list.Add(soapMethod.inHeaderMappings);
            if (soapMethod.outHeaderMappings != null)
            {
                list.Add(soapMethod.outHeaderMappings);
            }
            return list;
        }

        private void ImportHeaderSerializers(SoapReflectedMethod soapMethod)
        {
            List<SoapHeaderMapping> list = new List<SoapHeaderMapping>();
            List<SoapHeaderMapping> list2 = new List<SoapHeaderMapping>();
            for (int i = 0; i < soapMethod.headers.Length; i++)
            {
                SoapHeaderMapping item = new SoapHeaderMapping();
                SoapReflectedHeader header = soapMethod.headers[i];
                item.memberInfo = header.memberInfo;
                item.repeats = header.repeats;
                item.custom = header.custom;
                item.direction = header.direction;
                item.headerType = header.headerType;
                if (item.direction == SoapHeaderDirection.In)
                {
                    list.Add(item);
                }
                else if (item.direction == SoapHeaderDirection.Out)
                {
                    list2.Add(item);
                }
                else
                {
                    list.Add(item);
                    list2.Add(item);
                }
            }
            this.inHeaderMappings = list.ToArray();
            if (this.outHeaderSerializer != null)
            {
                this.outHeaderMappings = list2.ToArray();
            }
        }

        private void ImportReflectedMethod(SoapReflectedMethod soapMethod)
        {
            this.action = soapMethod.action;
            this.extensions = soapMethod.extensions;
            this.extensionInitializers = SoapReflectedExtension.GetInitializers(this.methodInfo, soapMethod.extensions);
            this.oneWay = soapMethod.oneWay;
            this.rpc = soapMethod.rpc;
            this.use = soapMethod.use;
            this.paramStyle = soapMethod.paramStyle;
            this.wsiClaims = (soapMethod.binding == null) ? WsiProfiles.None : soapMethod.binding.ConformsTo;
        }

        private void ImportSerializers(SoapReflectedMethod soapMethod, Evidence serverEvidence)
        {
            XmlMapping[] mappings = this.GetXmlMappingsForMethod(soapMethod).ToArray();
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "ImportSerializers", new object[0]) : null;
            if (Tracing.On)
            {
                Tracing.Enter(Tracing.TraceId("TraceCreateSerializer"), caller, new TraceMethod(typeof(XmlSerializer), "FromMappings", new object[] { mappings, serverEvidence }));
            }
            XmlSerializer[] serializerArray = null;
            if (AppDomain.CurrentDomain.IsHomogenous)
            {
                serializerArray = XmlSerializer.FromMappings(mappings);
            }
            else
            {
                serializerArray = XmlSerializer.FromMappings(mappings, serverEvidence);
            }
            if (Tracing.On)
            {
                Tracing.Exit(Tracing.TraceId("TraceCreateSerializer"), caller);
            }
            int num = 0;
            this.parameterSerializer = serializerArray[num++];
            if (soapMethod.responseMappings != null)
            {
                this.returnSerializer = serializerArray[num++];
            }
            this.inHeaderSerializer = serializerArray[num++];
            if (soapMethod.outHeaderMappings != null)
            {
                this.outHeaderSerializer = serializerArray[num++];
            }
        }

        public string Action
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.action;
            }
        }

        public SoapBindingUse BindingUse
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.use;
            }
        }

        public SoapHeaderMapping[] InHeaderMappings
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.inHeaderMappings;
            }
        }

        public XmlSerializer InHeaderSerializer
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.inHeaderSerializer;
            }
        }

        public LogicalMethodInfo MethodInfo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.methodInfo;
            }
        }

        public bool OneWay
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.oneWay;
            }
        }

        public SoapHeaderMapping[] OutHeaderMappings
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.outHeaderMappings;
            }
        }

        public XmlSerializer OutHeaderSerializer
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.outHeaderSerializer;
            }
        }

        public XmlSerializer ParameterSerializer
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parameterSerializer;
            }
        }

        public SoapParameterStyle ParameterStyle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.paramStyle;
            }
        }

        public XmlSerializer ReturnSerializer
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.returnSerializer;
            }
        }

        public bool Rpc
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.rpc;
            }
        }

        public WsiProfiles WsiClaims
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.wsiClaims;
            }
        }
    }
}

