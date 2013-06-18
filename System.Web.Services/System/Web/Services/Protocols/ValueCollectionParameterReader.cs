namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Runtime;
    using System.Web.Services;

    public abstract class ValueCollectionParameterReader : MimeParameterReader
    {
        private ParameterInfo[] paramInfos;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ValueCollectionParameterReader()
        {
        }

        public override object GetInitializer(LogicalMethodInfo methodInfo)
        {
            if (!IsSupported(methodInfo))
            {
                return null;
            }
            return methodInfo.InParameters;
        }

        public override void Initialize(object o)
        {
            this.paramInfos = (ParameterInfo[]) o;
        }

        public static bool IsSupported(ParameterInfo paramInfo)
        {
            Type parameterType = paramInfo.ParameterType;
            if (parameterType.IsArray)
            {
                parameterType = parameterType.GetElementType();
            }
            return ScalarFormatter.IsTypeSupported(parameterType);
        }

        public static bool IsSupported(LogicalMethodInfo methodInfo)
        {
            if (methodInfo.OutParameters.Length > 0)
            {
                return false;
            }
            ParameterInfo[] inParameters = methodInfo.InParameters;
            for (int i = 0; i < inParameters.Length; i++)
            {
                if (!IsSupported(inParameters[i]))
                {
                    return false;
                }
            }
            return true;
        }

        protected object[] Read(NameValueCollection collection)
        {
            object[] objArray = new object[this.paramInfos.Length];
            for (int i = 0; i < this.paramInfos.Length; i++)
            {
                ParameterInfo info = this.paramInfos[i];
                if (info.ParameterType.IsArray)
                {
                    string[] values = collection.GetValues(info.Name);
                    Type elementType = info.ParameterType.GetElementType();
                    Array array = Array.CreateInstance(elementType, values.Length);
                    for (int j = 0; j < values.Length; j++)
                    {
                        string str = values[j];
                        array.SetValue(ScalarFormatter.FromString(str, elementType), j);
                    }
                    objArray[i] = array;
                }
                else
                {
                    string str2 = collection[info.Name];
                    if (str2 == null)
                    {
                        throw new InvalidOperationException(Res.GetString("WebMissingParameter", new object[] { info.Name }));
                    }
                    objArray[i] = ScalarFormatter.FromString(str2, info.ParameterType);
                }
            }
            return objArray;
        }
    }
}

