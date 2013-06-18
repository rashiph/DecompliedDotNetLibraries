namespace System.Web.Services
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;

    internal class WebMethodReflector
    {
        private WebMethodReflector()
        {
        }

        internal static MethodInfo FindInterfaceMethodInfo(Type type, string signature)
        {
            foreach (Type type2 in type.GetInterfaces())
            {
                InterfaceMapping interfaceMap = type.GetInterfaceMap(type2);
                MethodInfo[] targetMethods = interfaceMap.TargetMethods;
                for (int i = 0; i < targetMethods.Length; i++)
                {
                    if (targetMethods[i].ToString() == signature)
                    {
                        return interfaceMap.InterfaceMethods[i];
                    }
                }
            }
            return null;
        }

        internal static WebMethodAttribute GetAttribute(MethodInfo implementation, MethodInfo declaration)
        {
            WebMethodAttribute attribute = null;
            WebMethodAttribute attribute2 = null;
            object[] customAttributes;
            if (declaration != null)
            {
                customAttributes = declaration.GetCustomAttributes(typeof(WebMethodAttribute), false);
                if (customAttributes.Length > 0)
                {
                    attribute = (WebMethodAttribute) customAttributes[0];
                }
            }
            customAttributes = implementation.GetCustomAttributes(typeof(WebMethodAttribute), false);
            if (customAttributes.Length > 0)
            {
                attribute2 = (WebMethodAttribute) customAttributes[0];
            }
            if (attribute == null)
            {
                return attribute2;
            }
            if (attribute2 == null)
            {
                return attribute;
            }
            if (attribute2.MessageNameSpecified)
            {
                throw new InvalidOperationException(Res.GetString("ContractOverride", new object[] { implementation.Name, implementation.DeclaringType.FullName, declaration.DeclaringType.FullName, declaration.ToString(), "WebMethod.MessageName" }));
            }
            return new WebMethodAttribute(attribute2.EnableSessionSpecified ? attribute2.EnableSession : attribute.EnableSession) { TransactionOption = attribute2.TransactionOptionSpecified ? attribute2.TransactionOption : attribute.TransactionOption, CacheDuration = attribute2.CacheDurationSpecified ? attribute2.CacheDuration : attribute.CacheDuration, BufferResponse = attribute2.BufferResponseSpecified ? attribute2.BufferResponse : attribute.BufferResponse, Description = attribute2.DescriptionSpecified ? attribute2.Description : attribute.Description };
        }

        internal static LogicalMethodInfo[] GetMethods(Type type)
        {
            if (type.IsInterface)
            {
                throw new InvalidOperationException(Res.GetString("NeedConcreteType", new object[] { type.FullName }));
            }
            ArrayList list = new ArrayList();
            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            Hashtable hashtable = new Hashtable();
            Hashtable declarations = new Hashtable();
            for (int i = 0; i < methods.Length; i++)
            {
                Type declaringType = methods[i].DeclaringType;
                if ((declaringType != typeof(object)) && (declaringType != typeof(WebService)))
                {
                    string signature = methods[i].ToString();
                    MethodInfo declaration = FindInterfaceMethodInfo(declaringType, signature);
                    WebServiceBindingAttribute binding = null;
                    if (declaration != null)
                    {
                        object[] customAttributes = declaration.DeclaringType.GetCustomAttributes(typeof(WebServiceBindingAttribute), false);
                        if (customAttributes.Length > 0)
                        {
                            if (customAttributes.Length > 1)
                            {
                                throw new ArgumentException(Res.GetString("OnlyOneWebServiceBindingAttributeMayBeSpecified1", new object[] { declaration.DeclaringType.FullName }), "type");
                            }
                            binding = (WebServiceBindingAttribute) customAttributes[0];
                            if ((binding.Name == null) || (binding.Name.Length == 0))
                            {
                                binding.Name = declaration.DeclaringType.Name;
                            }
                        }
                        else
                        {
                            declaration = null;
                        }
                    }
                    else if (!methods[i].IsPublic)
                    {
                        continue;
                    }
                    WebMethodAttribute attribute = GetAttribute(methods[i], declaration);
                    if (attribute != null)
                    {
                        WebMethod method = new WebMethod(declaration, binding, attribute);
                        declarations.Add(methods[i], method);
                        MethodInfo info2 = (MethodInfo) hashtable[signature];
                        if (info2 == null)
                        {
                            hashtable.Add(signature, methods[i]);
                            list.Add(methods[i]);
                        }
                        else if (info2.DeclaringType.IsAssignableFrom(methods[i].DeclaringType))
                        {
                            hashtable[signature] = methods[i];
                            list[list.IndexOf(info2)] = methods[i];
                        }
                    }
                }
            }
            return LogicalMethodInfo.Create((MethodInfo[]) list.ToArray(typeof(MethodInfo)), LogicalMethodTypes.Async | LogicalMethodTypes.Sync, declarations);
        }

        internal static void IncludeTypes(LogicalMethodInfo[] methods, XmlReflectionImporter importer)
        {
            for (int i = 0; i < methods.Length; i++)
            {
                LogicalMethodInfo method = methods[i];
                IncludeTypes(method, importer);
            }
        }

        internal static void IncludeTypes(LogicalMethodInfo method, XmlReflectionImporter importer)
        {
            if (method.Declaration != null)
            {
                importer.IncludeTypes(method.Declaration.DeclaringType);
                importer.IncludeTypes(method.Declaration);
            }
            importer.IncludeTypes(method.DeclaringType);
            importer.IncludeTypes(method.CustomAttributeProvider);
        }
    }
}

