namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Web.Services;
    using System.Web.Services.Diagnostics;
    using System.Xml.Serialization;

    internal class XmlReturn
    {
        private XmlReturn()
        {
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private static Evidence GetEvidenceForType(Type type)
        {
            return type.Assembly.Evidence;
        }

        internal static object GetInitializer(LogicalMethodInfo methodInfo)
        {
            return GetInitializers(new LogicalMethodInfo[] { methodInfo });
        }

        internal static object[] GetInitializers(LogicalMethodInfo[] methodInfos)
        {
            if (methodInfos.Length == 0)
            {
                return new object[0];
            }
            WebServiceAttribute attribute = WebServiceReflector.GetAttribute(methodInfos);
            bool serviceDefaultIsEncoded = SoapReflector.ServiceDefaultIsEncoded(WebServiceReflector.GetMostDerivedType(methodInfos));
            XmlReflectionImporter importer = SoapReflector.CreateXmlImporter(attribute.Namespace, serviceDefaultIsEncoded);
            WebMethodReflector.IncludeTypes(methodInfos, importer);
            ArrayList list = new ArrayList();
            bool[] flagArray = new bool[methodInfos.Length];
            for (int i = 0; i < methodInfos.Length; i++)
            {
                LogicalMethodInfo methodInfo = methodInfos[i];
                Type returnType = methodInfo.ReturnType;
                if (IsSupported(returnType) && HttpServerProtocol.AreUrlParametersSupported(methodInfo))
                {
                    XmlAttributes attributes = new XmlAttributes(methodInfo.ReturnTypeCustomAttributeProvider);
                    XmlTypeMapping mapping = importer.ImportTypeMapping(returnType, attributes.XmlRoot);
                    mapping.SetKey(methodInfo.GetKey() + ":Return");
                    list.Add(mapping);
                    flagArray[i] = true;
                }
            }
            if (list.Count == 0)
            {
                return new object[0];
            }
            XmlMapping[] mappings = (XmlMapping[]) list.ToArray(typeof(XmlMapping));
            Evidence evidenceForType = GetEvidenceForType(methodInfos[0].DeclaringType);
            TraceMethod caller = Tracing.On ? new TraceMethod(typeof(XmlReturn), "GetInitializers", methodInfos) : null;
            if (Tracing.On)
            {
                Tracing.Enter(Tracing.TraceId("TraceCreateSerializer"), caller, new TraceMethod(typeof(XmlSerializer), "FromMappings", new object[] { mappings, evidenceForType }));
            }
            XmlSerializer[] serializerArray = null;
            if (AppDomain.CurrentDomain.IsHomogenous)
            {
                serializerArray = XmlSerializer.FromMappings(mappings);
            }
            else
            {
                serializerArray = XmlSerializer.FromMappings(mappings, evidenceForType);
            }
            if (Tracing.On)
            {
                Tracing.Exit(Tracing.TraceId("TraceCreateSerializer"), caller);
            }
            object[] objArray = new object[methodInfos.Length];
            int num2 = 0;
            for (int j = 0; j < objArray.Length; j++)
            {
                if (flagArray[j])
                {
                    objArray[j] = serializerArray[num2++];
                }
            }
            return objArray;
        }

        private static bool IsSupported(Type returnType)
        {
            return (returnType != typeof(void));
        }
    }
}

