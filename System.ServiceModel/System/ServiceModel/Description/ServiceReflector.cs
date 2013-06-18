namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal static class ServiceReflector
    {
        private static readonly Type asyncCallbackType = typeof(AsyncCallback);
        private static readonly Type asyncResultType = typeof(IAsyncResult);
        internal const string BeginMethodNamePrefix = "Begin";
        internal const string EndMethodNamePrefix = "End";
        private static readonly Type objectType = typeof(object);
        private static readonly Type OperationContractAttributeType = typeof(OperationContractAttribute);
        internal const BindingFlags ServiceModelBindingFlags = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        internal static bool FlowsIn(ParameterInfo paramInfo)
        {
            if (paramInfo.IsOut)
            {
                return paramInfo.IsIn;
            }
            return true;
        }

        internal static bool FlowsOut(ParameterInfo paramInfo)
        {
            return paramInfo.ParameterType.IsByRef;
        }

        private static Type GetAncestorImplicitContractClass(Type service)
        {
            service = service.BaseType;
            while (service != null)
            {
                if (GetSingleAttribute<ServiceContractAttribute>(service) != null)
                {
                    return service;
                }
                service = service.BaseType;
            }
            return null;
        }

        internal static Type GetContractType(Type interfaceType)
        {
            ServiceContractAttribute attribute;
            return GetContractTypeAndAttribute(interfaceType, out attribute);
        }

        internal static Type GetContractTypeAndAttribute(Type interfaceType, out ServiceContractAttribute contractAttribute)
        {
            contractAttribute = GetSingleAttribute<ServiceContractAttribute>(interfaceType);
            if (contractAttribute != null)
            {
                return interfaceType;
            }
            List<Type> list = new List<Type>(GetInheritedContractTypes(interfaceType));
            if (list.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AttemptedToGetContractTypeForButThatTypeIs1", new object[] { interfaceType.Name })));
            }
            foreach (Type type in list)
            {
                bool flag = true;
                foreach (Type type2 in list)
                {
                    if (!type2.IsAssignableFrom(type))
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    contractAttribute = GetSingleAttribute<ServiceContractAttribute>(type);
                    return type;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxNoMostDerivedContract", new object[] { interfaceType.Name })));
        }

        internal static object[] GetCustomAttributes(ICustomAttributeProvider attrProvider, Type attrType)
        {
            return GetCustomAttributes(attrProvider, attrType, false);
        }

        internal static object[] GetCustomAttributes(ICustomAttributeProvider attrProvider, Type attrType, bool inherit)
        {
            object[] customAttributes;
            try
            {
                customAttributes = attrProvider.GetCustomAttributes(attrType, inherit);
            }
            catch (Exception innerException)
            {
                if (Fx.IsFatal(innerException))
                {
                    throw;
                }
                if ((innerException is CustomAttributeFormatException) && (innerException.InnerException != null))
                {
                    innerException = innerException.InnerException;
                    if ((innerException is TargetInvocationException) && (innerException.InnerException != null))
                    {
                        innerException = innerException.InnerException;
                    }
                }
                Type type = attrProvider as Type;
                MethodInfo member = attrProvider as MethodInfo;
                ParameterInfo info2 = attrProvider as ParameterInfo;
                if (type != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxErrorReflectingOnType2", new object[] { attrType.Name, type.Name }), innerException));
                }
                if (member != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxErrorReflectingOnMethod3", new object[] { attrType.Name, member.Name, member.ReflectedType.Name }), innerException));
                }
                if (info2 != null)
                {
                    member = info2.Member as MethodInfo;
                    if (member != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxErrorReflectingOnParameter4", new object[] { attrType.Name, info2.Name, member.Name, member.ReflectedType.Name }), innerException));
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxErrorReflectionOnUnknown1", new object[] { attrType.Name }), innerException));
            }
            return customAttributes;
        }

        internal static MethodInfo GetEndMethod(MethodInfo beginMethod)
        {
            MethodInfo endMethodInternal = GetEndMethodInternal(beginMethod);
            if (!HasEndMethodShape(endMethodInternal))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidAsyncEndMethodSignatureForMethod2", new object[] { endMethodInternal.Name, endMethodInternal.DeclaringType.FullName })));
            }
            return endMethodInternal;
        }

        private static MethodInfo GetEndMethodInternal(MethodInfo beginMethod)
        {
            string logicalName = GetLogicalName(beginMethod);
            string name = "End" + logicalName;
            MemberInfo[] member = beginMethod.DeclaringType.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (member.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoEndMethodFoundForAsyncBeginMethod3", new object[] { beginMethod.Name, beginMethod.DeclaringType.FullName, name })));
            }
            if (member.Length > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MoreThanOneEndMethodFoundForAsyncBeginMethod3", new object[] { beginMethod.Name, beginMethod.DeclaringType.FullName, name })));
            }
            return (MethodInfo) member[0];
        }

        internal static T GetFirstAttribute<T>(ICustomAttributeProvider attrProvider) where T: class
        {
            Type attrType = typeof(T);
            object[] customAttributes = GetCustomAttributes(attrProvider, attrType);
            if (customAttributes.Length == 0)
            {
                return default(T);
            }
            return (customAttributes[0] as T);
        }

        internal static List<Type> GetInheritedContractTypes(Type service)
        {
            List<Type> list = new List<Type>();
            foreach (Type type in service.GetInterfaces())
            {
                if (GetSingleAttribute<ServiceContractAttribute>(type) != null)
                {
                    list.Add(type);
                }
            }
            service = service.BaseType;
            while (service != null)
            {
                if (GetSingleAttribute<ServiceContractAttribute>(service) != null)
                {
                    list.Add(service);
                }
                service = service.BaseType;
            }
            return list;
        }

        internal static ParameterInfo[] GetInputParameters(MethodInfo method, bool asyncPattern)
        {
            int num = 0;
            ParameterInfo[] parameters = method.GetParameters();
            int length = parameters.Length;
            if (asyncPattern)
            {
                length -= 2;
            }
            for (int i = 0; i < length; i++)
            {
                if (FlowsIn(parameters[i]))
                {
                    num++;
                }
            }
            ParameterInfo[] infoArray2 = new ParameterInfo[num];
            int num4 = 0;
            for (int j = 0; j < length; j++)
            {
                ParameterInfo paramInfo = parameters[j];
                if (FlowsIn(paramInfo))
                {
                    infoArray2[num4++] = paramInfo;
                }
            }
            return infoArray2;
        }

        internal static List<Type> GetInterfaces(Type service)
        {
            List<Type> list = new List<Type>();
            bool flag = false;
            if (service.IsDefined(typeof(ServiceContractAttribute), false))
            {
                flag = true;
                list.Add(service);
            }
            if (!flag)
            {
                Type ancestorImplicitContractClass = GetAncestorImplicitContractClass(service);
                if (ancestorImplicitContractClass != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxContractInheritanceRequiresInterfaces2", new object[] { service, ancestorImplicitContractClass })));
                }
                foreach (MethodInfo info in GetMethodsInternal(service))
                {
                    Type operationContractProviderType = GetOperationContractProviderType(info);
                    if (operationContractProviderType == OperationContractAttributeType)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ServicesWithoutAServiceContractAttributeCan2", new object[] { operationContractProviderType.Name, info.Name, service.FullName })));
                    }
                }
            }
            foreach (Type type3 in service.GetInterfaces())
            {
                if (type3.IsDefined(typeof(ServiceContractAttribute), false))
                {
                    if (flag)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxContractInheritanceRequiresInterfaces", new object[] { service, type3 })));
                    }
                    list.Add(type3);
                }
            }
            return list;
        }

        internal static string GetLogicalName(MethodInfo method)
        {
            return GetLogicalName(method, IsBegin(method));
        }

        internal static string GetLogicalName(MethodInfo method, bool isAsync)
        {
            if (isAsync)
            {
                return method.Name.Substring("Begin".Length);
            }
            return method.Name;
        }

        private static List<MethodInfo> GetMethodsInternal(Type interfaceType)
        {
            List<MethodInfo> list = new List<MethodInfo>();
            foreach (MethodInfo info in interfaceType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (GetSingleAttribute<OperationContractAttribute>(info) != null)
                {
                    list.Add(info);
                }
                else if (GetFirstAttribute<IOperationContractAttributeProvider>(info) != null)
                {
                    list.Add(info);
                }
            }
            return list;
        }

        internal static OperationContractAttribute GetOperationContractAttribute(MethodInfo method)
        {
            OperationContractAttribute singleAttribute = GetSingleAttribute<OperationContractAttribute>(method);
            if (singleAttribute != null)
            {
                return singleAttribute;
            }
            IOperationContractAttributeProvider firstAttribute = GetFirstAttribute<IOperationContractAttributeProvider>(method);
            if (firstAttribute != null)
            {
                return firstAttribute.GetOperationContractAttribute();
            }
            return null;
        }

        internal static Type GetOperationContractProviderType(MethodInfo method)
        {
            if (GetSingleAttribute<OperationContractAttribute>(method) != null)
            {
                return OperationContractAttributeType;
            }
            IOperationContractAttributeProvider firstAttribute = GetFirstAttribute<IOperationContractAttributeProvider>(method);
            if (firstAttribute != null)
            {
                return firstAttribute.GetType();
            }
            return null;
        }

        internal static XmlName GetOperationName(MethodInfo method)
        {
            OperationContractAttribute operationContractAttribute = GetOperationContractAttribute(method);
            return NamingHelper.GetOperationName(GetLogicalName(method), operationContractAttribute.Name);
        }

        internal static ParameterInfo[] GetOutputParameters(MethodInfo method, bool asyncPattern)
        {
            int num = 0;
            ParameterInfo[] parameters = method.GetParameters();
            int length = parameters.Length;
            if (asyncPattern)
            {
                length--;
            }
            for (int i = 0; i < length; i++)
            {
                if (FlowsOut(parameters[i]))
                {
                    num++;
                }
            }
            ParameterInfo[] infoArray2 = new ParameterInfo[num];
            int num4 = 0;
            for (int j = 0; j < length; j++)
            {
                ParameterInfo paramInfo = parameters[j];
                if (FlowsOut(paramInfo))
                {
                    infoArray2[num4++] = paramInfo;
                }
            }
            return infoArray2;
        }

        internal static T GetRequiredSingleAttribute<T>(ICustomAttributeProvider attrProvider) where T: class
        {
            T singleAttribute = GetSingleAttribute<T>(attrProvider);
            if (singleAttribute == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("couldnTFindRequiredAttributeOfTypeOn2", new object[] { typeof(T), attrProvider.ToString() })));
            }
            return singleAttribute;
        }

        internal static T GetRequiredSingleAttribute<T>(ICustomAttributeProvider attrProvider, Type[] attrTypeGroup) where T: class
        {
            T singleAttribute = GetSingleAttribute<T>(attrProvider, attrTypeGroup);
            if (singleAttribute == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("couldnTFindRequiredAttributeOfTypeOn2", new object[] { typeof(T), attrProvider.ToString() })));
            }
            return singleAttribute;
        }

        internal static T GetSingleAttribute<T>(ICustomAttributeProvider attrProvider) where T: class
        {
            Type attrType = typeof(T);
            object[] customAttributes = GetCustomAttributes(attrProvider, attrType);
            if (customAttributes.Length == 0)
            {
                return default(T);
            }
            if (customAttributes.Length > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("tooManyAttributesOfTypeOn2", new object[] { attrType, attrProvider.ToString() })));
            }
            return (customAttributes[0] as T);
        }

        internal static T GetSingleAttribute<T>(ICustomAttributeProvider attrProvider, Type[] attrTypeGroup) where T: class
        {
            T singleAttribute = GetSingleAttribute<T>(attrProvider);
            if (singleAttribute != null)
            {
                Type type = typeof(T);
                foreach (Type type2 in attrTypeGroup)
                {
                    if (!(type2 == type))
                    {
                        object[] customAttributes = GetCustomAttributes(attrProvider, type2);
                        if ((customAttributes != null) && (customAttributes.Length > 0))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxDisallowedAttributeCombination", new object[] { attrProvider, type.FullName, type2.FullName })));
                        }
                    }
                }
            }
            return singleAttribute;
        }

        internal static bool HasBeginMethodShape(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            return ((method.Name.StartsWith("Begin", StringComparison.Ordinal) && (parameters.Length >= 2)) && ((!(parameters[parameters.Length - 2].ParameterType != asyncCallbackType) && !(parameters[parameters.Length - 1].ParameterType != objectType)) && !(method.ReturnType != asyncResultType)));
        }

        internal static bool HasEndMethodShape(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            return ((method.Name.StartsWith("End", StringComparison.Ordinal) && (parameters.Length >= 1)) && !(parameters[parameters.Length - 1].ParameterType != asyncResultType));
        }

        internal static bool HasNoDisposableParameters(MethodInfo methodInfo)
        {
            foreach (ParameterInfo info in methodInfo.GetParameters())
            {
                if (IsParameterDisposable(info.ParameterType))
                {
                    return false;
                }
            }
            if (methodInfo.ReturnParameter != null)
            {
                return !IsParameterDisposable(methodInfo.ReturnParameter.ParameterType);
            }
            return true;
        }

        internal static bool HasOutputParameters(MethodInfo method, bool asyncPattern)
        {
            ParameterInfo[] parameters = method.GetParameters();
            int length = parameters.Length;
            if (asyncPattern)
            {
                length--;
            }
            for (int i = 0; i < length; i++)
            {
                if (FlowsOut(parameters[i]))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsBegin(MethodInfo method)
        {
            OperationContractAttribute operationContractAttribute = GetOperationContractAttribute(method);
            if (operationContractAttribute == null)
            {
                return false;
            }
            return IsBegin(operationContractAttribute, method);
        }

        internal static bool IsBegin(OperationContractAttribute opSettings, MethodInfo method)
        {
            if (!opSettings.AsyncPattern)
            {
                return false;
            }
            if (!HasBeginMethodShape(method))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidAsyncBeginMethodSignatureForMethod2", new object[] { method.Name, method.DeclaringType.FullName })));
            }
            return true;
        }

        internal static bool IsParameterDisposable(Type type)
        {
            if (type.IsSealed)
            {
                return typeof(IDisposable).IsAssignableFrom(type);
            }
            return true;
        }

        internal static void ValidateParameterMetadata(MethodInfo methodInfo)
        {
            foreach (ParameterInfo info in methodInfo.GetParameters())
            {
                if (!info.ParameterType.IsByRef)
                {
                    if (info.IsOut)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBadByValueParameterMetadata", new object[] { methodInfo.Name, methodInfo.DeclaringType.Name })));
                    }
                }
                else if (info.IsIn && !info.IsOut)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBadByReferenceParameterMetadata", new object[] { methodInfo.Name, methodInfo.DeclaringType.Name })));
                }
            }
        }
    }
}

