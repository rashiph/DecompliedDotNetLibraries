namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Workflow.Runtime;

    internal sealed class DefaultCorrelationProvider : ICorrelationProvider
    {
        private Dictionary<string, CorrelationPropertyValue[]> cachedCorrelationProperties = new Dictionary<string, CorrelationPropertyValue[]>();
        private object cachedCorrelationPropertiesSync = new object();
        private Dictionary<string, bool> initializerCorrelationPropertys;
        private object initializerCorrelationPropertysSync = new object();
        private Type interfaceType;

        internal DefaultCorrelationProvider(Type interfaceType)
        {
            this.interfaceType = interfaceType;
        }

        private object[] GetCorrelationParameterAttributes(Type type)
        {
            return type.GetCustomAttributes(typeof(CorrelationParameterAttribute), true);
        }

        private CorrelationPropertyValue[] GetCorrelationProperties(Type interfaceType, string methodName)
        {
            CorrelationPropertyValue[] valueArray = null;
            if (interfaceType.GetCustomAttributes(typeof(ExternalDataExchangeAttribute), true).Length == 0)
            {
                throw new InvalidOperationException(SR.GetString("Error_ExternalDataExchangeException", new object[] { interfaceType.AssemblyQualifiedName }));
            }
            List<object> list = new List<object>();
            list.AddRange(this.GetCorrelationParameterAttributes(interfaceType));
            if (list.Count == 0)
            {
                throw new InvalidOperationException(SR.GetString("Error_CorrelationParameterException", new object[] { interfaceType.AssemblyQualifiedName }));
            }
            valueArray = new CorrelationPropertyValue[list.Count];
            Dictionary<string, CorrelationAliasAttribute> correlationAliases = null;
            MethodInfo methodInfo = null;
            this.GetMethodInfo(interfaceType, methodName, out methodInfo, out correlationAliases);
            if (methodInfo == null)
            {
                throw new MissingMethodException(interfaceType.AssemblyQualifiedName, methodName);
            }
            ParameterInfo[] parameters = methodInfo.GetParameters();
            int num = 0;
            foreach (CorrelationParameterAttribute attribute in list)
            {
                string name = attribute.Name;
                CorrelationAliasAttribute attribute2 = this.GetMatchingCorrelationAlias(attribute, correlationAliases, list.Count == 1);
                if (attribute2 != null)
                {
                    name = attribute2.Path;
                }
                CorrelationPropertyValue value2 = this.GetCorrelationProperty(parameters, attribute.Name, name);
                if (value2 == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CorrelationParameterException", new object[] { interfaceType.AssemblyQualifiedName, attribute.Name, methodName }));
                }
                valueArray[num++] = value2;
            }
            return valueArray;
        }

        private CorrelationPropertyValue GetCorrelationProperty(ParameterInfo[] parameters, string propertyName, string location)
        {
            string[] strArray = location.Split(new char[] { '.' });
            if (((strArray.Length == 1) && (parameters.Length == 2)) && typeof(ExternalDataEventArgs).IsAssignableFrom(parameters[1].ParameterType))
            {
                string str = "e." + location;
                return this.GetCorrelationProperty(parameters, propertyName, "e", str);
            }
            string parameterName = strArray[0];
            return this.GetCorrelationProperty(parameters, propertyName, parameterName, location);
        }

        private CorrelationPropertyValue GetCorrelationProperty(ParameterInfo[] parameters, string propertyName, string parameterName, string location)
        {
            for (int i = 0; (parameters != null) && (i < parameters.Length); i++)
            {
                ParameterInfo info = parameters[i];
                if (info.Name == parameterName)
                {
                    return new CorrelationPropertyValue(propertyName, location, info.Position);
                }
            }
            return null;
        }

        private CorrelationAliasAttribute GetMatchingCorrelationAlias(CorrelationParameterAttribute paramAttribute, Dictionary<string, CorrelationAliasAttribute> correlationAliases, bool defaultParameter)
        {
            CorrelationAliasAttribute attribute = null;
            if (correlationAliases == null)
            {
                return null;
            }
            if (!defaultParameter || !correlationAliases.TryGetValue("", out attribute))
            {
                correlationAliases.TryGetValue(paramAttribute.Name, out attribute);
            }
            return attribute;
        }

        private void GetMethodInfo(Type interfaceType, string methodName, out MethodInfo methodInfo, out Dictionary<string, CorrelationAliasAttribute> correlationAliases)
        {
            correlationAliases = new Dictionary<string, CorrelationAliasAttribute>();
            object[] customAttributes = null;
            methodInfo = null;
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            EventInfo info = interfaceType.GetEvent(methodName, bindingAttr);
            if (info != null)
            {
                customAttributes = info.GetCustomAttributes(typeof(CorrelationAliasAttribute), true);
                if ((customAttributes == null) || (customAttributes.Length == 0))
                {
                    customAttributes = info.EventHandlerType.GetCustomAttributes(typeof(CorrelationAliasAttribute), true);
                }
                MethodInfo[] methods = info.EventHandlerType.GetMethods();
                methodInfo = methods[0];
            }
            else
            {
                methodInfo = interfaceType.GetMethod(methodName, bindingAttr);
                if (methodInfo == null)
                {
                    throw new MissingMethodException(interfaceType.AssemblyQualifiedName, methodName);
                }
                customAttributes = methodInfo.GetCustomAttributes(typeof(CorrelationAliasAttribute), true);
            }
            foreach (CorrelationAliasAttribute attribute in customAttributes)
            {
                if ((customAttributes.Length > 1) && (attribute.Name == null))
                {
                    throw new ArgumentNullException("ParameterName");
                }
                correlationAliases.Add((attribute.Name == null) ? "" : attribute.Name, attribute);
            }
        }

        bool ICorrelationProvider.IsInitializingMember(Type interfaceType, string memberName, object[] methodArgs)
        {
            return this.InitializerCorrelationPropertys.ContainsKey(memberName);
        }

        ICollection<CorrelationProperty> ICorrelationProvider.ResolveCorrelationPropertyValues(Type interfaceType, string methodName, object[] methodArgs, bool provideInitializerTokens)
        {
            CorrelationPropertyValue[] correlationProperties = null;
            if ((methodArgs == null) || provideInitializerTokens)
            {
                return null;
            }
            this.cachedCorrelationProperties.TryGetValue(methodName, out correlationProperties);
            if (correlationProperties == null)
            {
                lock (this.cachedCorrelationPropertiesSync)
                {
                    this.cachedCorrelationProperties.TryGetValue(methodName, out correlationProperties);
                    if (correlationProperties == null)
                    {
                        correlationProperties = this.GetCorrelationProperties(interfaceType, methodName);
                        this.cachedCorrelationProperties.Add(methodName, correlationProperties);
                    }
                }
            }
            List<CorrelationProperty> list = new List<CorrelationProperty>();
            for (int i = 0; i < correlationProperties.Length; i++)
            {
                list.Add(new CorrelationProperty(correlationProperties[i].Name, correlationProperties[i].GetValue(methodArgs)));
            }
            return list;
        }

        private Dictionary<string, bool> InitializerCorrelationPropertys
        {
            get
            {
                if (this.initializerCorrelationPropertys == null)
                {
                    lock (this.initializerCorrelationPropertysSync)
                    {
                        if (this.initializerCorrelationPropertys == null)
                        {
                            Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
                            foreach (EventInfo info in this.interfaceType.GetEvents())
                            {
                                if (info.GetCustomAttributes(typeof(CorrelationInitializerAttribute), true).Length > 0)
                                {
                                    dictionary.Add(info.Name, true);
                                }
                            }
                            foreach (MethodInfo info2 in this.interfaceType.GetMethods())
                            {
                                if (info2.GetCustomAttributes(typeof(CorrelationInitializerAttribute), true).Length > 0)
                                {
                                    dictionary.Add(info2.Name, false);
                                }
                            }
                            this.initializerCorrelationPropertys = dictionary;
                        }
                    }
                }
                return this.initializerCorrelationPropertys;
            }
        }
    }
}

