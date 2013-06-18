namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Services;

    public sealed class LogicalMethodInfo
    {
        private WebMethodAttribute attribute;
        private Hashtable attributes;
        private WebServiceBindingAttribute binding;
        private ParameterInfo callbackParam;
        private System.Reflection.MethodInfo declaration;
        private static object[] emptyObjectArray = new object[0];
        private System.Reflection.MethodInfo endMethodInfo;
        private static System.Security.Cryptography.HashAlgorithm hash;
        private ParameterInfo[] inParams;
        private bool isVoid;
        private System.Reflection.MethodInfo methodInfo;
        private string methodName;
        private ParameterInfo[] outParams;
        private ParameterInfo[] parameters;
        private ParameterInfo resultParam;
        private Type retType;
        private ParameterInfo stateParam;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public LogicalMethodInfo(System.Reflection.MethodInfo methodInfo) : this(methodInfo, null)
        {
        }

        internal LogicalMethodInfo(System.Reflection.MethodInfo methodInfo, WebMethod webMethod)
        {
            if (methodInfo.IsStatic)
            {
                throw new InvalidOperationException(Res.GetString("WebMethodStatic", new object[] { methodInfo.Name }));
            }
            this.methodInfo = methodInfo;
            if (webMethod != null)
            {
                this.binding = webMethod.binding;
                this.attribute = webMethod.attribute;
                this.declaration = webMethod.declaration;
            }
            System.Reflection.MethodInfo info = (this.declaration != null) ? this.declaration : methodInfo;
            this.parameters = info.GetParameters();
            this.inParams = GetInParameters(info, this.parameters, 0, this.parameters.Length, false);
            this.outParams = GetOutParameters(info, this.parameters, 0, this.parameters.Length, false);
            this.retType = info.ReturnType;
            this.isVoid = this.retType == typeof(void);
            this.methodName = info.Name;
            this.attributes = new Hashtable();
        }

        private LogicalMethodInfo(System.Reflection.MethodInfo beginMethodInfo, System.Reflection.MethodInfo endMethodInfo, WebMethod webMethod)
        {
            this.methodInfo = beginMethodInfo;
            this.endMethodInfo = endMethodInfo;
            this.methodName = beginMethodInfo.Name.Substring(5);
            if (webMethod != null)
            {
                this.binding = webMethod.binding;
                this.attribute = webMethod.attribute;
                this.declaration = webMethod.declaration;
            }
            ParameterInfo[] parameters = beginMethodInfo.GetParameters();
            if (((parameters.Length < 2) || (parameters[parameters.Length - 1].ParameterType != typeof(object))) || (parameters[parameters.Length - 2].ParameterType != typeof(AsyncCallback)))
            {
                throw new InvalidOperationException(Res.GetString("WebMethodMissingParams", new object[] { beginMethodInfo.DeclaringType.FullName, beginMethodInfo.Name, typeof(AsyncCallback).FullName, typeof(object).FullName }));
            }
            this.stateParam = parameters[parameters.Length - 1];
            this.callbackParam = parameters[parameters.Length - 2];
            this.inParams = GetInParameters(beginMethodInfo, parameters, 0, parameters.Length - 2, true);
            ParameterInfo[] paramInfos = endMethodInfo.GetParameters();
            this.resultParam = paramInfos[0];
            this.outParams = GetOutParameters(endMethodInfo, paramInfos, 1, paramInfos.Length - 1, true);
            this.parameters = new ParameterInfo[this.inParams.Length + this.outParams.Length];
            this.inParams.CopyTo(this.parameters, 0);
            this.outParams.CopyTo(this.parameters, this.inParams.Length);
            this.retType = endMethodInfo.ReturnType;
            this.isVoid = this.retType == typeof(void);
            this.attributes = new Hashtable();
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public IAsyncResult BeginInvoke(object target, object[] values, AsyncCallback callback, object asyncState)
        {
            object[] array = new object[values.Length + 2];
            values.CopyTo(array, 0);
            array[values.Length] = callback;
            array[values.Length + 1] = asyncState;
            return (IAsyncResult) this.methodInfo.Invoke(target, array);
        }

        internal static bool CanMerge(Type type)
        {
            return ((type == typeof(SoapHeaderAttribute)) || typeof(SoapExtensionAttribute).IsAssignableFrom(type));
        }

        internal void CheckContractOverride()
        {
            if (this.declaration != null)
            {
                this.methodInfo.GetParameters();
                foreach (ParameterInfo info in this.methodInfo.GetParameters())
                {
                    foreach (object obj2 in info.GetCustomAttributes(false))
                    {
                        if (obj2.GetType().Namespace == "System.Xml.Serialization")
                        {
                            throw new InvalidOperationException(Res.GetString("ContractOverride", new object[] { this.methodInfo.Name, this.methodInfo.DeclaringType.FullName, this.declaration.DeclaringType.FullName, this.declaration.ToString(), obj2.ToString() }));
                        }
                    }
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static LogicalMethodInfo[] Create(System.Reflection.MethodInfo[] methodInfos)
        {
            return Create(methodInfos, LogicalMethodTypes.Async | LogicalMethodTypes.Sync, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static LogicalMethodInfo[] Create(System.Reflection.MethodInfo[] methodInfos, LogicalMethodTypes types)
        {
            return Create(methodInfos, types, null);
        }

        internal static LogicalMethodInfo[] Create(System.Reflection.MethodInfo[] methodInfos, LogicalMethodTypes types, Hashtable declarations)
        {
            ArrayList list = ((types & LogicalMethodTypes.Async) != ((LogicalMethodTypes) 0)) ? new ArrayList() : null;
            Hashtable hashtable = ((types & LogicalMethodTypes.Async) != ((LogicalMethodTypes) 0)) ? new Hashtable() : null;
            ArrayList list2 = ((types & LogicalMethodTypes.Sync) != ((LogicalMethodTypes) 0)) ? new ArrayList() : null;
            for (int i = 0; i < methodInfos.Length; i++)
            {
                System.Reflection.MethodInfo methodInfo = methodInfos[i];
                if (IsBeginMethod(methodInfo))
                {
                    if (list != null)
                    {
                        list.Add(methodInfo);
                    }
                }
                else if (IsEndMethod(methodInfo))
                {
                    if (hashtable != null)
                    {
                        hashtable.Add(methodInfo.Name, methodInfo);
                    }
                }
                else if (list2 != null)
                {
                    list2.Add(methodInfo);
                }
            }
            int num2 = (list == null) ? 0 : list.Count;
            int num3 = (list2 == null) ? 0 : list2.Count;
            int index = num3 + num2;
            LogicalMethodInfo[] infoArray = new LogicalMethodInfo[index];
            index = 0;
            for (int j = 0; j < num3; j++)
            {
                System.Reflection.MethodInfo info2 = (System.Reflection.MethodInfo) list2[j];
                WebMethod webMethod = (declarations == null) ? null : ((WebMethod) declarations[info2]);
                infoArray[index] = new LogicalMethodInfo(info2, webMethod);
                infoArray[index].CheckContractOverride();
                index++;
            }
            for (int k = 0; k < num2; k++)
            {
                System.Reflection.MethodInfo beginMethodInfo = (System.Reflection.MethodInfo) list[k];
                string str = "End" + beginMethodInfo.Name.Substring(5);
                System.Reflection.MethodInfo endMethodInfo = (System.Reflection.MethodInfo) hashtable[str];
                if (endMethodInfo == null)
                {
                    throw new InvalidOperationException(Res.GetString("WebAsyncMissingEnd", new object[] { beginMethodInfo.DeclaringType.FullName, beginMethodInfo.Name, str }));
                }
                WebMethod method2 = (declarations == null) ? null : ((WebMethod) declarations[beginMethodInfo]);
                infoArray[index++] = new LogicalMethodInfo(beginMethodInfo, endMethodInfo, method2);
            }
            return infoArray;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public object[] EndInvoke(object target, IAsyncResult asyncResult)
        {
            object[] parameters = new object[this.outParams.Length + 1];
            parameters[0] = asyncResult;
            object obj2 = this.endMethodInfo.Invoke(target, parameters);
            if (!this.isVoid)
            {
                parameters[0] = obj2;
                return parameters;
            }
            if (this.outParams.Length > 0)
            {
                object[] destinationArray = new object[this.outParams.Length];
                Array.Copy(parameters, 1, destinationArray, 0, destinationArray.Length);
                return destinationArray;
            }
            return emptyObjectArray;
        }

        public object GetCustomAttribute(Type type)
        {
            object[] customAttributes = this.GetCustomAttributes(type);
            if (customAttributes.Length == 0)
            {
                return null;
            }
            return customAttributes[0];
        }

        public object[] GetCustomAttributes(Type type)
        {
            object[] objArray = null;
            objArray = (object[]) this.attributes[type];
            if (objArray == null)
            {
                lock (this.attributes)
                {
                    objArray = (object[]) this.attributes[type];
                    if (objArray != null)
                    {
                        return objArray;
                    }
                    if (this.declaration != null)
                    {
                        object[] customAttributes = this.declaration.GetCustomAttributes(type, false);
                        object[] objArray3 = this.methodInfo.GetCustomAttributes(type, false);
                        if (objArray3.Length > 0)
                        {
                            if (!CanMerge(type))
                            {
                                throw new InvalidOperationException(Res.GetString("ContractOverride", new object[] { this.methodInfo.Name, this.methodInfo.DeclaringType.FullName, this.declaration.DeclaringType.FullName, this.declaration.ToString(), objArray3[0].ToString() }));
                            }
                            ArrayList list = new ArrayList();
                            for (int i = 0; i < customAttributes.Length; i++)
                            {
                                list.Add(customAttributes[i]);
                            }
                            for (int j = 0; j < objArray3.Length; j++)
                            {
                                list.Add(objArray3[j]);
                            }
                            objArray = (object[]) list.ToArray(type);
                        }
                        else
                        {
                            objArray = customAttributes;
                        }
                    }
                    else
                    {
                        objArray = this.methodInfo.GetCustomAttributes(type, false);
                    }
                    this.attributes[type] = objArray;
                }
            }
            return objArray;
        }

        private static ParameterInfo[] GetInParameters(System.Reflection.MethodInfo methodInfo, ParameterInfo[] paramInfos, int start, int length, bool mustBeIn)
        {
            int num = 0;
            for (int i = 0; i < length; i++)
            {
                ParameterInfo paramInfo = paramInfos[i + start];
                if (IsInParameter(paramInfo))
                {
                    num++;
                }
                else if (mustBeIn)
                {
                    throw new InvalidOperationException(Res.GetString("WebBadOutParameter", new object[] { paramInfo.Name, methodInfo.DeclaringType.FullName, paramInfo.Name }));
                }
            }
            ParameterInfo[] infoArray = new ParameterInfo[num];
            num = 0;
            for (int j = 0; j < length; j++)
            {
                ParameterInfo info2 = paramInfos[j + start];
                if (IsInParameter(info2))
                {
                    infoArray[num++] = info2;
                }
            }
            return infoArray;
        }

        internal string GetKey()
        {
            if (this.methodInfo == null)
            {
                return string.Empty;
            }
            string s = this.methodInfo.DeclaringType.FullName + ":" + this.methodInfo.ToString();
            if (s.Length > 0x400)
            {
                s = Convert.ToBase64String(HashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(s)));
            }
            return s;
        }

        private static ParameterInfo[] GetOutParameters(System.Reflection.MethodInfo methodInfo, ParameterInfo[] paramInfos, int start, int length, bool mustBeOut)
        {
            int num = 0;
            for (int i = 0; i < length; i++)
            {
                ParameterInfo paramInfo = paramInfos[i + start];
                if (IsOutParameter(paramInfo))
                {
                    num++;
                }
                else if (mustBeOut)
                {
                    throw new InvalidOperationException(Res.GetString("WebInOutParameter", new object[] { paramInfo.Name, methodInfo.DeclaringType.FullName, paramInfo.Name }));
                }
            }
            ParameterInfo[] infoArray = new ParameterInfo[num];
            num = 0;
            for (int j = 0; j < length; j++)
            {
                ParameterInfo info2 = paramInfos[j + start];
                if (IsOutParameter(info2))
                {
                    infoArray[num++] = info2;
                }
            }
            return infoArray;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public object[] Invoke(object target, object[] values)
        {
            if (this.outParams.Length > 0)
            {
                object[] objArray = new object[this.parameters.Length];
                for (int i = 0; i < this.inParams.Length; i++)
                {
                    objArray[this.inParams[i].Position] = values[i];
                }
                values = objArray;
            }
            object obj2 = this.methodInfo.Invoke(target, values);
            if (this.outParams.Length > 0)
            {
                int length = this.outParams.Length;
                if (!this.isVoid)
                {
                    length++;
                }
                object[] objArray2 = new object[length];
                length = 0;
                if (!this.isVoid)
                {
                    objArray2[length++] = obj2;
                }
                for (int j = 0; j < this.outParams.Length; j++)
                {
                    objArray2[length++] = values[this.outParams[j].Position];
                }
                return objArray2;
            }
            if (this.isVoid)
            {
                return emptyObjectArray;
            }
            return new object[] { obj2 };
        }

        public static bool IsBeginMethod(System.Reflection.MethodInfo methodInfo)
        {
            return (typeof(IAsyncResult).IsAssignableFrom(methodInfo.ReturnType) && methodInfo.Name.StartsWith("Begin", StringComparison.Ordinal));
        }

        public static bool IsEndMethod(System.Reflection.MethodInfo methodInfo)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            return (((parameters.Length > 0) && typeof(IAsyncResult).IsAssignableFrom(parameters[0].ParameterType)) && methodInfo.Name.StartsWith("End", StringComparison.Ordinal));
        }

        private static bool IsInParameter(ParameterInfo paramInfo)
        {
            return !paramInfo.IsOut;
        }

        private static bool IsOutParameter(ParameterInfo paramInfo)
        {
            if (!paramInfo.IsOut)
            {
                return paramInfo.ParameterType.IsByRef;
            }
            return true;
        }

        public override string ToString()
        {
            return this.methodInfo.ToString();
        }

        public ParameterInfo AsyncCallbackParameter
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.callbackParam;
            }
        }

        public ParameterInfo AsyncResultParameter
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.resultParam;
            }
        }

        public ParameterInfo AsyncStateParameter
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.stateParam;
            }
        }

        public System.Reflection.MethodInfo BeginMethodInfo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.methodInfo;
            }
        }

        internal WebServiceBindingAttribute Binding
        {
            get
            {
                return this.binding;
            }
        }

        public ICustomAttributeProvider CustomAttributeProvider
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.methodInfo;
            }
        }

        internal System.Reflection.MethodInfo Declaration
        {
            get
            {
                return this.declaration;
            }
        }

        public Type DeclaringType
        {
            get
            {
                return this.methodInfo.DeclaringType;
            }
        }

        public System.Reflection.MethodInfo EndMethodInfo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.endMethodInfo;
            }
        }

        internal static System.Security.Cryptography.HashAlgorithm HashAlgorithm
        {
            get
            {
                if (hash == null)
                {
                    hash = SHA1.Create();
                }
                return hash;
            }
        }

        public ParameterInfo[] InParameters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.inParams;
            }
        }

        public bool IsAsync
        {
            get
            {
                return (this.endMethodInfo != null);
            }
        }

        public bool IsVoid
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isVoid;
            }
        }

        internal WebMethodAttribute MethodAttribute
        {
            get
            {
                if (this.attribute == null)
                {
                    this.attribute = (WebMethodAttribute) this.GetCustomAttribute(typeof(WebMethodAttribute));
                    if (this.attribute == null)
                    {
                        this.attribute = new WebMethodAttribute();
                    }
                }
                return this.attribute;
            }
        }

        public System.Reflection.MethodInfo MethodInfo
        {
            get
            {
                if (this.endMethodInfo != null)
                {
                    return null;
                }
                return this.methodInfo;
            }
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.methodName;
            }
        }

        public ParameterInfo[] OutParameters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.outParams;
            }
        }

        public ParameterInfo[] Parameters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parameters;
            }
        }

        public Type ReturnType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.retType;
            }
        }

        public ICustomAttributeProvider ReturnTypeCustomAttributeProvider
        {
            get
            {
                if (this.declaration != null)
                {
                    return this.declaration.ReturnTypeCustomAttributes;
                }
                return this.methodInfo.ReturnTypeCustomAttributes;
            }
        }
    }
}

