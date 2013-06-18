namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Threading;
    using System.Web.Services;

    internal class HttpClientType
    {
        private Hashtable methods = new Hashtable();

        internal HttpClientType(Type type)
        {
            LogicalMethodInfo[] infoArray = LogicalMethodInfo.Create(type.GetMethods(), LogicalMethodTypes.Sync);
            Hashtable formatterTypes = new Hashtable();
            for (int i = 0; i < infoArray.Length; i++)
            {
                LogicalMethodInfo info = infoArray[i];
                try
                {
                    object[] customAttributes = info.GetCustomAttributes(typeof(HttpMethodAttribute));
                    if (customAttributes.Length != 0)
                    {
                        HttpMethodAttribute attribute = (HttpMethodAttribute) customAttributes[0];
                        HttpClientMethod method = new HttpClientMethod {
                            readerType = attribute.ReturnFormatter,
                            writerType = attribute.ParameterFormatter,
                            methodInfo = info
                        };
                        AddFormatter(formatterTypes, method.readerType, method);
                        AddFormatter(formatterTypes, method.writerType, method);
                        this.methods.Add(info.Name, method);
                    }
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    throw new InvalidOperationException(Res.GetString("WebReflectionError", new object[] { info.DeclaringType.FullName, info.Name }), exception);
                }
            }
            foreach (Type type2 in formatterTypes.Keys)
            {
                ArrayList list = (ArrayList) formatterTypes[type2];
                LogicalMethodInfo[] methodInfos = new LogicalMethodInfo[list.Count];
                for (int j = 0; j < list.Count; j++)
                {
                    methodInfos[j] = ((HttpClientMethod) list[j]).methodInfo;
                }
                object[] initializers = MimeFormatter.GetInitializers(type2, methodInfos);
                bool flag = typeof(MimeParameterWriter).IsAssignableFrom(type2);
                for (int k = 0; k < list.Count; k++)
                {
                    if (flag)
                    {
                        ((HttpClientMethod) list[k]).writerInitializer = initializers[k];
                    }
                    else
                    {
                        ((HttpClientMethod) list[k]).readerInitializer = initializers[k];
                    }
                }
            }
        }

        private static void AddFormatter(Hashtable formatterTypes, Type formatterType, HttpClientMethod method)
        {
            if (formatterType != null)
            {
                ArrayList list = (ArrayList) formatterTypes[formatterType];
                if (list == null)
                {
                    list = new ArrayList();
                    formatterTypes.Add(formatterType, list);
                }
                list.Add(method);
            }
        }

        internal HttpClientMethod GetMethod(string name)
        {
            return (HttpClientMethod) this.methods[name];
        }
    }
}

