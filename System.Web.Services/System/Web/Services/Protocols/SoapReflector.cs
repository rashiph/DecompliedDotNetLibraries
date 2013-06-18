namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Serialization;

    internal static class SoapReflector
    {
        internal static SoapReflectionImporter CreateSoapImporter(string defaultNs, bool serviceDefaultIsEncoded)
        {
            return new SoapReflectionImporter(GetEncodedNamespace(defaultNs, serviceDefaultIsEncoded));
        }

        internal static XmlReflectionImporter CreateXmlImporter(string defaultNs, bool serviceDefaultIsEncoded)
        {
            return new XmlReflectionImporter(GetLiteralNamespace(defaultNs, serviceDefaultIsEncoded));
        }

        private static string GetDefaultAction(string defaultNs, LogicalMethodInfo methodInfo)
        {
            string messageName = methodInfo.MethodAttribute.MessageName;
            if (messageName.Length == 0)
            {
                messageName = methodInfo.Name;
            }
            if (defaultNs.EndsWith("/", StringComparison.Ordinal))
            {
                return (defaultNs + messageName);
            }
            return (defaultNs + "/" + messageName);
        }

        internal static string GetEncodedNamespace(string ns, bool serviceDefaultIsEncoded)
        {
            if (serviceDefaultIsEncoded)
            {
                return ns;
            }
            if (ns.EndsWith("/", StringComparison.Ordinal))
            {
                return (ns + "encodedTypes");
            }
            return (ns + "/encodedTypes");
        }

        internal static string GetLiteralNamespace(string ns, bool serviceDefaultIsEncoded)
        {
            if (!serviceDefaultIsEncoded)
            {
                return ns;
            }
            if (ns.EndsWith("/", StringComparison.Ordinal))
            {
                return (ns + "literalTypes");
            }
            return (ns + "/literalTypes");
        }

        internal static object GetSoapMethodAttribute(LogicalMethodInfo methodInfo)
        {
            object[] customAttributes = methodInfo.GetCustomAttributes(typeof(SoapRpcMethodAttribute));
            object[] objArray2 = methodInfo.GetCustomAttributes(typeof(SoapDocumentMethodAttribute));
            if (customAttributes.Length > 0)
            {
                if (objArray2.Length > 0)
                {
                    throw new ArgumentException(System.Web.Services.Res.GetString("WebBothMethodAttrs"), "methodInfo");
                }
                return customAttributes[0];
            }
            if (objArray2.Length > 0)
            {
                return objArray2[0];
            }
            return null;
        }

        internal static string GetSoapMethodBinding(LogicalMethodInfo method)
        {
            string binding;
            object[] customAttributes = method.GetCustomAttributes(typeof(SoapDocumentMethodAttribute));
            if (customAttributes.Length == 0)
            {
                customAttributes = method.GetCustomAttributes(typeof(SoapRpcMethodAttribute));
                if (customAttributes.Length == 0)
                {
                    binding = string.Empty;
                }
                else
                {
                    binding = ((SoapRpcMethodAttribute) customAttributes[0]).Binding;
                }
            }
            else
            {
                binding = ((SoapDocumentMethodAttribute) customAttributes[0]).Binding;
            }
            if (method.Binding == null)
            {
                return binding;
            }
            if ((binding.Length > 0) && (binding != method.Binding.Name))
            {
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebInvalidBindingName", new object[] { binding, method.Binding.Name }));
            }
            return method.Binding.Name;
        }

        internal static object GetSoapServiceAttribute(Type type)
        {
            object[] customAttributes = type.GetCustomAttributes(typeof(SoapRpcServiceAttribute), false);
            object[] objArray2 = type.GetCustomAttributes(typeof(SoapDocumentServiceAttribute), false);
            if (customAttributes.Length > 0)
            {
                if (objArray2.Length > 0)
                {
                    throw new ArgumentException(System.Web.Services.Res.GetString("WebBothServiceAttrs"), "methodInfo");
                }
                return customAttributes[0];
            }
            if (objArray2.Length > 0)
            {
                return objArray2[0];
            }
            return null;
        }

        internal static SoapServiceRoutingStyle GetSoapServiceRoutingStyle(object soapServiceAttribute)
        {
            if (soapServiceAttribute is SoapRpcServiceAttribute)
            {
                return ((SoapRpcServiceAttribute) soapServiceAttribute).RoutingStyle;
            }
            if (soapServiceAttribute is SoapDocumentServiceAttribute)
            {
                return ((SoapDocumentServiceAttribute) soapServiceAttribute).RoutingStyle;
            }
            return SoapServiceRoutingStyle.SoapAction;
        }

        private static Exception HeaderException(string memberName, Type declaringType, string description)
        {
            return new Exception(System.Web.Services.Res.GetString(description, new object[] { declaringType.Name, memberName }));
        }

        private static XmlMembersMapping ImportMembersMapping(XmlReflectionImporter xmlImporter, SoapReflectionImporter soapImporter, bool serviceDefaultIsEncoded, bool rpc, SoapBindingUse use, SoapParameterStyle paramStyle, string elementName, string elementNamespace, bool nsIsDefault, XmlReflectionMember[] members, bool validate, bool openModel, string key, bool writeAccess)
        {
            XmlMembersMapping mapping = null;
            if (use == SoapBindingUse.Encoded)
            {
                string ns = ((!rpc && (paramStyle != SoapParameterStyle.Bare)) && nsIsDefault) ? GetEncodedNamespace(elementNamespace, serviceDefaultIsEncoded) : elementNamespace;
                mapping = soapImporter.ImportMembersMapping(elementName, ns, members, rpc || (paramStyle != SoapParameterStyle.Bare), rpc, validate, writeAccess ? XmlMappingAccess.Write : XmlMappingAccess.Read);
            }
            else
            {
                string str2 = nsIsDefault ? GetLiteralNamespace(elementNamespace, serviceDefaultIsEncoded) : elementNamespace;
                mapping = xmlImporter.ImportMembersMapping(elementName, str2, members, paramStyle != SoapParameterStyle.Bare, rpc, openModel, writeAccess ? XmlMappingAccess.Write : XmlMappingAccess.Read);
            }
            if (mapping != null)
            {
                mapping.SetKey(key);
            }
            return mapping;
        }

        internal static void IncludeTypes(LogicalMethodInfo[] methods, SoapReflectionImporter importer)
        {
            for (int i = 0; i < methods.Length; i++)
            {
                LogicalMethodInfo method = methods[i];
                IncludeTypes(method, importer);
            }
        }

        internal static void IncludeTypes(LogicalMethodInfo method, SoapReflectionImporter importer)
        {
            if (method.Declaration != null)
            {
                importer.IncludeTypes(method.Declaration.DeclaringType);
                importer.IncludeTypes(method.Declaration);
            }
            importer.IncludeTypes(method.DeclaringType);
            importer.IncludeTypes(method.CustomAttributeProvider);
        }

        internal static SoapReflectedMethod ReflectMethod(LogicalMethodInfo methodInfo, bool client, XmlReflectionImporter xmlImporter, SoapReflectionImporter soapImporter, string defaultNs)
        {
            SoapReflectedMethod method2;
            try
            {
                string str2;
                string str4;
                string key = methodInfo.GetKey();
                SoapReflectedMethod method = new SoapReflectedMethod();
                MethodAttribute attribute = new MethodAttribute();
                object soapServiceAttribute = GetSoapServiceAttribute(methodInfo.DeclaringType);
                bool serviceDefaultIsEncoded = ServiceDefaultIsEncoded(soapServiceAttribute);
                object soapMethodAttribute = GetSoapMethodAttribute(methodInfo);
                if (soapMethodAttribute == null)
                {
                    if (client)
                    {
                        return null;
                    }
                    if (soapServiceAttribute is SoapRpcServiceAttribute)
                    {
                        SoapRpcMethodAttribute attribute2 = new SoapRpcMethodAttribute {
                            Use = ((SoapRpcServiceAttribute) soapServiceAttribute).Use
                        };
                        soapMethodAttribute = attribute2;
                    }
                    else if (soapServiceAttribute is SoapDocumentServiceAttribute)
                    {
                        SoapDocumentMethodAttribute attribute3 = new SoapDocumentMethodAttribute {
                            Use = ((SoapDocumentServiceAttribute) soapServiceAttribute).Use
                        };
                        soapMethodAttribute = attribute3;
                    }
                    else
                    {
                        soapMethodAttribute = new SoapDocumentMethodAttribute();
                    }
                }
                if (soapMethodAttribute is SoapRpcMethodAttribute)
                {
                    SoapRpcMethodAttribute attribute4 = (SoapRpcMethodAttribute) soapMethodAttribute;
                    method.rpc = true;
                    method.use = attribute4.Use;
                    method.oneWay = attribute4.OneWay;
                    attribute.action = attribute4.Action;
                    attribute.binding = attribute4.Binding;
                    attribute.requestName = attribute4.RequestElementName;
                    attribute.requestNs = attribute4.RequestNamespace;
                    attribute.responseName = attribute4.ResponseElementName;
                    attribute.responseNs = attribute4.ResponseNamespace;
                }
                else
                {
                    SoapDocumentMethodAttribute attribute5 = (SoapDocumentMethodAttribute) soapMethodAttribute;
                    method.rpc = false;
                    method.use = attribute5.Use;
                    method.paramStyle = attribute5.ParameterStyle;
                    method.oneWay = attribute5.OneWay;
                    attribute.action = attribute5.Action;
                    attribute.binding = attribute5.Binding;
                    attribute.requestName = attribute5.RequestElementName;
                    attribute.requestNs = attribute5.RequestNamespace;
                    attribute.responseName = attribute5.ResponseElementName;
                    attribute.responseNs = attribute5.ResponseNamespace;
                    if (method.use == SoapBindingUse.Default)
                    {
                        if (soapServiceAttribute is SoapDocumentServiceAttribute)
                        {
                            method.use = ((SoapDocumentServiceAttribute) soapServiceAttribute).Use;
                        }
                        if (method.use == SoapBindingUse.Default)
                        {
                            method.use = SoapBindingUse.Literal;
                        }
                    }
                    if (method.paramStyle == SoapParameterStyle.Default)
                    {
                        if (soapServiceAttribute is SoapDocumentServiceAttribute)
                        {
                            method.paramStyle = ((SoapDocumentServiceAttribute) soapServiceAttribute).ParameterStyle;
                        }
                        if (method.paramStyle == SoapParameterStyle.Default)
                        {
                            method.paramStyle = SoapParameterStyle.Wrapped;
                        }
                    }
                }
                if (attribute.binding.Length > 0)
                {
                    if (client)
                    {
                        throw new InvalidOperationException(System.Web.Services.Res.GetString("WebInvalidBindingPlacement", new object[] { soapMethodAttribute.GetType().Name }));
                    }
                    method.binding = WebServiceBindingReflector.GetAttribute(methodInfo, attribute.binding);
                }
                WebMethodAttribute methodAttribute = methodInfo.MethodAttribute;
                method.name = methodAttribute.MessageName;
                if (method.name.Length == 0)
                {
                    method.name = methodInfo.Name;
                }
                if (method.rpc)
                {
                    str2 = ((attribute.requestName.Length == 0) || !client) ? methodInfo.Name : attribute.requestName;
                }
                else
                {
                    str2 = (attribute.requestName.Length == 0) ? method.name : attribute.requestName;
                }
                string requestNs = attribute.requestNs;
                if (requestNs == null)
                {
                    if (((method.binding != null) && (method.binding.Namespace != null)) && (method.binding.Namespace.Length != 0))
                    {
                        requestNs = method.binding.Namespace;
                    }
                    else
                    {
                        requestNs = defaultNs;
                    }
                }
                if (method.rpc && (method.use != SoapBindingUse.Encoded))
                {
                    str4 = methodInfo.Name + "Response";
                }
                else
                {
                    str4 = (attribute.responseName.Length == 0) ? (method.name + "Response") : attribute.responseName;
                }
                string responseNs = attribute.responseNs;
                if (responseNs == null)
                {
                    if (((method.binding != null) && (method.binding.Namespace != null)) && (method.binding.Namespace.Length != 0))
                    {
                        responseNs = method.binding.Namespace;
                    }
                    else
                    {
                        responseNs = defaultNs;
                    }
                }
                SoapParameterInfo[] infoArray = ReflectParameters(methodInfo.InParameters, requestNs);
                SoapParameterInfo[] infoArray2 = ReflectParameters(methodInfo.OutParameters, responseNs);
                method.action = attribute.action;
                if (method.action == null)
                {
                    method.action = GetDefaultAction(defaultNs, methodInfo);
                }
                method.methodInfo = methodInfo;
                if (method.oneWay)
                {
                    if (infoArray2.Length > 0)
                    {
                        throw new ArgumentException(System.Web.Services.Res.GetString("WebOneWayOutParameters"), "methodInfo");
                    }
                    if (methodInfo.ReturnType != typeof(void))
                    {
                        throw new ArgumentException(System.Web.Services.Res.GetString("WebOneWayReturnValue"), "methodInfo");
                    }
                }
                XmlReflectionMember[] members = new XmlReflectionMember[infoArray.Length];
                for (int i = 0; i < members.Length; i++)
                {
                    SoapParameterInfo info = infoArray[i];
                    XmlReflectionMember member = new XmlReflectionMember {
                        MemberName = info.parameterInfo.Name,
                        MemberType = info.parameterInfo.ParameterType
                    };
                    if (member.MemberType.IsByRef)
                    {
                        member.MemberType = member.MemberType.GetElementType();
                    }
                    member.XmlAttributes = info.xmlAttributes;
                    member.SoapAttributes = info.soapAttributes;
                    members[i] = member;
                }
                method.requestMappings = ImportMembersMapping(xmlImporter, soapImporter, serviceDefaultIsEncoded, method.rpc, method.use, method.paramStyle, str2, requestNs, attribute.requestNs == null, members, true, false, key, client);
                if (((GetSoapServiceRoutingStyle(soapServiceAttribute) == SoapServiceRoutingStyle.RequestElement) && (method.paramStyle == SoapParameterStyle.Bare)) && (method.requestMappings.Count != 1))
                {
                    throw new ArgumentException(System.Web.Services.Res.GetString("WhenUsingAMessageStyleOfParametersAsDocument0"), "methodInfo");
                }
                string name = "";
                string ns = "";
                if (method.paramStyle == SoapParameterStyle.Bare)
                {
                    if (method.requestMappings.Count == 1)
                    {
                        name = method.requestMappings[0].XsdElementName;
                        ns = method.requestMappings[0].Namespace;
                    }
                }
                else
                {
                    name = method.requestMappings.XsdElementName;
                    ns = method.requestMappings.Namespace;
                }
                method.requestElementName = new XmlQualifiedName(name, ns);
                if (!method.oneWay)
                {
                    int num2 = infoArray2.Length;
                    int num3 = 0;
                    CodeIdentifiers identifiers = null;
                    if (methodInfo.ReturnType != typeof(void))
                    {
                        num2++;
                        num3 = 1;
                        identifiers = new CodeIdentifiers();
                    }
                    members = new XmlReflectionMember[num2];
                    for (int m = 0; m < infoArray2.Length; m++)
                    {
                        SoapParameterInfo info2 = infoArray2[m];
                        XmlReflectionMember member2 = new XmlReflectionMember {
                            MemberName = info2.parameterInfo.Name,
                            MemberType = info2.parameterInfo.ParameterType
                        };
                        if (member2.MemberType.IsByRef)
                        {
                            member2.MemberType = member2.MemberType.GetElementType();
                        }
                        member2.XmlAttributes = info2.xmlAttributes;
                        member2.SoapAttributes = info2.soapAttributes;
                        members[num3++] = member2;
                        if (identifiers != null)
                        {
                            identifiers.Add(member2.MemberName, null);
                        }
                    }
                    if (methodInfo.ReturnType != typeof(void))
                    {
                        XmlReflectionMember member3 = new XmlReflectionMember {
                            MemberName = identifiers.MakeUnique(method.name + "Result"),
                            MemberType = methodInfo.ReturnType,
                            IsReturnValue = true,
                            XmlAttributes = new XmlAttributes(methodInfo.ReturnTypeCustomAttributeProvider)
                        };
                        member3.XmlAttributes.XmlRoot = null;
                        member3.SoapAttributes = new SoapAttributes(methodInfo.ReturnTypeCustomAttributeProvider);
                        members[0] = member3;
                    }
                    method.responseMappings = ImportMembersMapping(xmlImporter, soapImporter, serviceDefaultIsEncoded, method.rpc, method.use, method.paramStyle, str4, responseNs, attribute.responseNs == null, members, false, false, key + ":Response", !client);
                }
                SoapExtensionAttribute[] customAttributes = (SoapExtensionAttribute[]) methodInfo.GetCustomAttributes(typeof(SoapExtensionAttribute));
                method.extensions = new SoapReflectedExtension[customAttributes.Length];
                for (int j = 0; j < customAttributes.Length; j++)
                {
                    method.extensions[j] = new SoapReflectedExtension(customAttributes[j].ExtensionType, customAttributes[j]);
                }
                Array.Sort<SoapReflectedExtension>(method.extensions);
                SoapHeaderAttribute[] array = (SoapHeaderAttribute[]) methodInfo.GetCustomAttributes(typeof(SoapHeaderAttribute));
                Array.Sort(array, new SoapHeaderAttributeComparer());
                Hashtable hashtable = new Hashtable();
                method.headers = new SoapReflectedHeader[array.Length];
                int num6 = 0;
                int length = method.headers.Length;
                ArrayList list = new ArrayList();
                ArrayList list2 = new ArrayList();
                for (int k = 0; k < method.headers.Length; k++)
                {
                    SoapHeaderAttribute attribute7 = array[k];
                    SoapReflectedHeader header = new SoapReflectedHeader();
                    Type declaringType = methodInfo.DeclaringType;
                    header.memberInfo = declaringType.GetField(attribute7.MemberName);
                    if (header.memberInfo != null)
                    {
                        header.headerType = ((FieldInfo) header.memberInfo).FieldType;
                    }
                    else
                    {
                        header.memberInfo = declaringType.GetProperty(attribute7.MemberName);
                        if (header.memberInfo == null)
                        {
                            throw HeaderException(attribute7.MemberName, methodInfo.DeclaringType, "WebHeaderMissing");
                        }
                        header.headerType = ((PropertyInfo) header.memberInfo).PropertyType;
                    }
                    if (header.headerType.IsArray)
                    {
                        header.headerType = header.headerType.GetElementType();
                        header.repeats = true;
                        if ((header.headerType != typeof(SoapUnknownHeader)) && (header.headerType != typeof(SoapHeader)))
                        {
                            throw HeaderException(attribute7.MemberName, methodInfo.DeclaringType, "WebHeaderType");
                        }
                    }
                    if (MemberHelper.IsStatic(header.memberInfo))
                    {
                        throw HeaderException(attribute7.MemberName, methodInfo.DeclaringType, "WebHeaderStatic");
                    }
                    if (!MemberHelper.CanRead(header.memberInfo))
                    {
                        throw HeaderException(attribute7.MemberName, methodInfo.DeclaringType, "WebHeaderRead");
                    }
                    if (!MemberHelper.CanWrite(header.memberInfo))
                    {
                        throw HeaderException(attribute7.MemberName, methodInfo.DeclaringType, "WebHeaderWrite");
                    }
                    if (!typeof(SoapHeader).IsAssignableFrom(header.headerType))
                    {
                        throw HeaderException(attribute7.MemberName, methodInfo.DeclaringType, "WebHeaderType");
                    }
                    SoapHeaderDirection direction = attribute7.Direction;
                    if (method.oneWay && ((direction & (SoapHeaderDirection.Fault | SoapHeaderDirection.Out)) != 0))
                    {
                        throw HeaderException(attribute7.MemberName, methodInfo.DeclaringType, "WebHeaderOneWayOut");
                    }
                    if (hashtable.Contains(header.headerType))
                    {
                        SoapHeaderDirection direction2 = (SoapHeaderDirection) hashtable[header.headerType];
                        if ((direction2 & direction) != 0)
                        {
                            throw HeaderException(attribute7.MemberName, methodInfo.DeclaringType, "WebMultiplyDeclaredHeaderTypes");
                        }
                        hashtable[header.headerType] = direction | direction2;
                    }
                    else
                    {
                        hashtable[header.headerType] = direction;
                    }
                    if ((header.headerType != typeof(SoapHeader)) && (header.headerType != typeof(SoapUnknownHeader)))
                    {
                        XmlReflectionMember member4 = new XmlReflectionMember {
                            MemberName = header.headerType.Name,
                            MemberType = header.headerType
                        };
                        XmlAttributes attributes = new XmlAttributes(header.headerType);
                        if (attributes.XmlRoot != null)
                        {
                            member4.XmlAttributes = new XmlAttributes();
                            XmlElementAttribute attribute8 = new XmlElementAttribute {
                                ElementName = attributes.XmlRoot.ElementName,
                                Namespace = attributes.XmlRoot.Namespace
                            };
                            member4.XmlAttributes.XmlElements.Add(attribute8);
                        }
                        member4.OverrideIsNullable = true;
                        if ((direction & SoapHeaderDirection.In) != 0)
                        {
                            list.Add(member4);
                        }
                        if ((direction & (SoapHeaderDirection.Fault | SoapHeaderDirection.Out)) != 0)
                        {
                            list2.Add(member4);
                        }
                        header.custom = true;
                    }
                    header.direction = direction;
                    if (!header.custom)
                    {
                        method.headers[--length] = header;
                    }
                    else
                    {
                        method.headers[num6++] = header;
                    }
                }
                method.inHeaderMappings = ImportMembersMapping(xmlImporter, soapImporter, serviceDefaultIsEncoded, false, method.use, SoapParameterStyle.Bare, str2 + "InHeaders", defaultNs, true, (XmlReflectionMember[]) list.ToArray(typeof(XmlReflectionMember)), false, true, key + ":InHeaders", client);
                if (!method.oneWay)
                {
                    method.outHeaderMappings = ImportMembersMapping(xmlImporter, soapImporter, serviceDefaultIsEncoded, false, method.use, SoapParameterStyle.Bare, str4 + "OutHeaders", defaultNs, true, (XmlReflectionMember[]) list2.ToArray(typeof(XmlReflectionMember)), false, true, key + ":OutHeaders", !client);
                }
                method2 = method;
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebReflectionErrorMethod", new object[] { methodInfo.DeclaringType.Name, methodInfo.Name }), exception);
            }
            return method2;
        }

        private static SoapParameterInfo[] ReflectParameters(ParameterInfo[] paramInfos, string ns)
        {
            SoapParameterInfo[] infoArray = new SoapParameterInfo[paramInfos.Length];
            for (int i = 0; i < paramInfos.Length; i++)
            {
                SoapParameterInfo info = new SoapParameterInfo();
                ParameterInfo provider = paramInfos[i];
                if (provider.ParameterType.IsArray && (provider.ParameterType.GetArrayRank() > 1))
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebMultiDimArray"));
                }
                info.xmlAttributes = new XmlAttributes(provider);
                info.soapAttributes = new SoapAttributes(provider);
                info.parameterInfo = provider;
                infoArray[i] = info;
            }
            return infoArray;
        }

        internal static bool ServiceDefaultIsEncoded(object soapServiceAttribute)
        {
            if (soapServiceAttribute == null)
            {
                return false;
            }
            if (soapServiceAttribute is SoapDocumentServiceAttribute)
            {
                return (((SoapDocumentServiceAttribute) soapServiceAttribute).Use == SoapBindingUse.Encoded);
            }
            return ((soapServiceAttribute is SoapRpcServiceAttribute) && (((SoapRpcServiceAttribute) soapServiceAttribute).Use == SoapBindingUse.Encoded));
        }

        internal static bool ServiceDefaultIsEncoded(Type type)
        {
            return ServiceDefaultIsEncoded(GetSoapServiceAttribute(type));
        }

        private class MethodAttribute
        {
            internal string action;
            internal string binding;
            internal string requestName;
            internal string requestNs;
            internal string responseName;
            internal string responseNs;
        }

        private class SoapParameterInfo
        {
            internal ParameterInfo parameterInfo;
            internal SoapAttributes soapAttributes;
            internal XmlAttributes xmlAttributes;
        }
    }
}

