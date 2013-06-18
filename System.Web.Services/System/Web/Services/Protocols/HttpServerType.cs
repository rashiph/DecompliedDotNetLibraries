namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Web.Services;
    using System.Web.Services.Configuration;

    internal class HttpServerType : ServerType
    {
        private Hashtable methods;

        internal HttpServerType(Type type) : base(type)
        {
            this.methods = new Hashtable();
            WebServicesSection current = WebServicesSection.Current;
            Type[] returnWriterTypes = current.ReturnWriterTypes;
            Type[] parameterReaderTypes = current.ParameterReaderTypes;
            LogicalMethodInfo[] methods = WebMethodReflector.GetMethods(type);
            HttpServerMethod[] methodArray = new HttpServerMethod[methods.Length];
            object[] objArray = new object[returnWriterTypes.Length];
            for (int i = 0; i < objArray.Length; i++)
            {
                objArray[i] = MimeFormatter.GetInitializers(returnWriterTypes[i], methods);
            }
            for (int j = 0; j < methods.Length; j++)
            {
                LogicalMethodInfo info = methods[j];
                HttpServerMethod method = null;
                if (info.ReturnType == typeof(void))
                {
                    method = new HttpServerMethod();
                }
                else
                {
                    for (int num3 = 0; num3 < returnWriterTypes.Length; num3++)
                    {
                        object[] objArray2 = (object[]) objArray[num3];
                        if (objArray2[j] != null)
                        {
                            method = new HttpServerMethod {
                                writerInitializer = objArray2[j],
                                writerType = returnWriterTypes[num3]
                            };
                            break;
                        }
                    }
                }
                if (method != null)
                {
                    method.methodInfo = info;
                    methodArray[j] = method;
                }
            }
            objArray = new object[parameterReaderTypes.Length];
            for (int k = 0; k < objArray.Length; k++)
            {
                objArray[k] = MimeFormatter.GetInitializers(parameterReaderTypes[k], methods);
            }
            for (int m = 0; m < methods.Length; m++)
            {
                HttpServerMethod method2 = methodArray[m];
                if (method2 != null)
                {
                    LogicalMethodInfo info2 = methods[m];
                    if (info2.InParameters.Length > 0)
                    {
                        int index = 0;
                        for (int num7 = 0; num7 < parameterReaderTypes.Length; num7++)
                        {
                            object[] objArray3 = (object[]) objArray[num7];
                            if (objArray3[m] != null)
                            {
                                index++;
                            }
                        }
                        if (index == 0)
                        {
                            methodArray[m] = null;
                        }
                        else
                        {
                            method2.readerTypes = new Type[index];
                            method2.readerInitializers = new object[index];
                            index = 0;
                            for (int num8 = 0; num8 < parameterReaderTypes.Length; num8++)
                            {
                                object[] objArray4 = (object[]) objArray[num8];
                                if (objArray4[m] != null)
                                {
                                    method2.readerTypes[index] = parameterReaderTypes[num8];
                                    method2.readerInitializers[index] = objArray4[m];
                                    index++;
                                }
                            }
                        }
                    }
                }
            }
            for (int n = 0; n < methodArray.Length; n++)
            {
                HttpServerMethod method3 = methodArray[n];
                if (method3 != null)
                {
                    WebMethodAttribute methodAttribute = method3.methodInfo.MethodAttribute;
                    method3.name = methodAttribute.MessageName;
                    if (method3.name.Length == 0)
                    {
                        method3.name = method3.methodInfo.Name;
                    }
                    this.methods.Add(method3.name, method3);
                }
            }
        }

        internal HttpServerMethod GetMethod(string name)
        {
            return (HttpServerMethod) this.methods[name];
        }

        internal HttpServerMethod GetMethodIgnoreCase(string name)
        {
            foreach (DictionaryEntry entry in this.methods)
            {
                HttpServerMethod method = (HttpServerMethod) entry.Value;
                if (string.Compare(method.name, name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return method;
                }
            }
            return null;
        }
    }
}

