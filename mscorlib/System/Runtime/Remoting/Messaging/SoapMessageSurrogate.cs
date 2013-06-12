namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Text;

    internal class SoapMessageSurrogate : ISerializationSurrogate
    {
        private object _rootObj;
        private static Type _soapFaultType = typeof(SoapFault);
        [SecurityCritical]
        private RemotingSurrogateSelector _ss;
        private static Type _voidType = typeof(void);
        private string DefaultFakeRecordAssemblyName = "http://schemas.microsoft.com/urt/SystemRemotingSoapTopRecord";

        [SecurityCritical]
        internal SoapMessageSurrogate(RemotingSurrogateSelector ss)
        {
            this._ss = ss;
        }

        [SecurityCritical]
        internal virtual string[] GetInArgNames(IMethodCallMessage m, int c)
        {
            string[] strArray = new string[c];
            for (int i = 0; i < c; i++)
            {
                string inArgName = m.GetInArgName(i);
                if (inArgName == null)
                {
                    inArgName = "__param" + i;
                }
                strArray[i] = inArgName;
            }
            return strArray;
        }

        [SecurityCritical]
        internal virtual string[] GetNames(IMethodCallMessage m, int c)
        {
            string[] strArray = new string[c];
            for (int i = 0; i < c; i++)
            {
                string argName = m.GetArgName(i);
                if (argName == null)
                {
                    argName = "__param" + i;
                }
                strArray[i] = argName;
            }
            return strArray;
        }

        [SecurityCritical]
        public virtual void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            if ((obj != null) && (obj != this._rootObj))
            {
                new MessageSurrogate(this._ss).GetObjectData(obj, info, context);
            }
            else
            {
                IMethodReturnMessage mm = obj as IMethodReturnMessage;
                if (mm != null)
                {
                    if (mm.Exception != null)
                    {
                        object data = CallContext.GetData("__ClientIsClr");
                        bool flag = (data == null) || ((bool) data);
                        info.FullTypeName = "FormatterWrapper";
                        info.AssemblyName = this.DefaultFakeRecordAssemblyName;
                        Exception innerException = mm.Exception;
                        StringBuilder builder = new StringBuilder();
                        bool flag2 = false;
                        while (innerException != null)
                        {
                            if (innerException.Message.StartsWith("MustUnderstand", StringComparison.Ordinal))
                            {
                                flag2 = true;
                            }
                            builder.Append(" **** ");
                            builder.Append(innerException.GetType().FullName);
                            builder.Append(" - ");
                            builder.Append(innerException.Message);
                            innerException = innerException.InnerException;
                        }
                        ServerFault serverFault = null;
                        if (flag)
                        {
                            serverFault = new ServerFault(mm.Exception);
                        }
                        else
                        {
                            serverFault = new ServerFault(mm.Exception.GetType().AssemblyQualifiedName, builder.ToString(), mm.Exception.StackTrace);
                        }
                        string faultCode = "Server";
                        if (flag2)
                        {
                            faultCode = "MustUnderstand";
                        }
                        SoapFault fault2 = new SoapFault(faultCode, builder.ToString(), null, serverFault);
                        info.AddValue("__WrappedObject", fault2, _soapFaultType);
                    }
                    else
                    {
                        MethodBase methodBase = mm.MethodBase;
                        SoapMethodAttribute cachedSoapAttribute = (SoapMethodAttribute) InternalRemotingServices.GetCachedSoapAttribute(methodBase);
                        string responseXmlElementName = cachedSoapAttribute.ResponseXmlElementName;
                        string responseXmlNamespace = cachedSoapAttribute.ResponseXmlNamespace;
                        string returnXmlElementName = cachedSoapAttribute.ReturnXmlElementName;
                        ArgMapper mapper = new ArgMapper(mm, true);
                        object[] args = mapper.Args;
                        info.FullTypeName = responseXmlElementName;
                        info.AssemblyName = responseXmlNamespace;
                        Type returnType = ((MethodInfo) methodBase).ReturnType;
                        if ((returnType != null) && !(returnType == _voidType))
                        {
                            info.AddValue(returnXmlElementName, mm.ReturnValue, returnType);
                        }
                        if (args != null)
                        {
                            Type[] argTypes = mapper.ArgTypes;
                            for (int i = 0; i < args.Length; i++)
                            {
                                string argName = mapper.GetArgName(i);
                                if ((argName == null) || (argName.Length == 0))
                                {
                                    argName = "__param" + i;
                                }
                                info.AddValue(argName, args[i], argTypes[i].IsByRef ? argTypes[i].GetElementType() : argTypes[i]);
                            }
                        }
                    }
                }
                else
                {
                    IMethodCallMessage m = (IMethodCallMessage) obj;
                    MethodBase mb = m.MethodBase;
                    string xmlNamespaceForMethodCall = SoapServices.GetXmlNamespaceForMethodCall(mb);
                    object[] inArgs = m.InArgs;
                    string[] inArgNames = this.GetInArgNames(m, inArgs.Length);
                    Type[] methodSignature = (Type[]) m.MethodSignature;
                    info.FullTypeName = m.MethodName;
                    info.AssemblyName = xmlNamespaceForMethodCall;
                    int[] marshalRequestArgMap = InternalRemotingServices.GetReflectionCachedData(mb).MarshalRequestArgMap;
                    for (int j = 0; j < inArgs.Length; j++)
                    {
                        string name = null;
                        if ((inArgNames[j] == null) || (inArgNames[j].Length == 0))
                        {
                            name = "__param" + j;
                        }
                        else
                        {
                            name = inArgNames[j];
                        }
                        int index = marshalRequestArgMap[j];
                        Type type = null;
                        if (methodSignature[index].IsByRef)
                        {
                            type = methodSignature[index].GetElementType();
                        }
                        else
                        {
                            type = methodSignature[index];
                        }
                        info.AddValue(name, inArgs[j], type);
                    }
                }
            }
        }

        [SecurityCritical]
        public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_PopulateData"));
        }

        internal void SetRootObject(object obj)
        {
            this._rootObj = obj;
        }
    }
}

