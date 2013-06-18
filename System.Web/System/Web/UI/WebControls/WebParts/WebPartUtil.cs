namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Reflection;
    using System.Web;
    using System.Web.Compilation;

    internal static class WebPartUtil
    {
        internal static object CreateObjectFromType(Type type)
        {
            return HttpRuntime.FastCreatePublicInstance(type);
        }

        internal static Type DeserializeType(string typeName, bool throwOnError)
        {
            return BuildManager.GetType(typeName, throwOnError);
        }

        internal static Type[] GetTypesForConstructor(ConstructorInfo constructor)
        {
            ParameterInfo[] parameters = constructor.GetParameters();
            Type[] typeArray = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                typeArray[i] = parameters[i].ParameterType;
            }
            return typeArray;
        }

        internal static bool IsConnectionPointTypeValid(Type connectionPointType, bool isConsumer)
        {
            if (connectionPointType != null)
            {
                if (!connectionPointType.IsPublic && !connectionPointType.IsNestedPublic)
                {
                    return false;
                }
                Type c = isConsumer ? typeof(ConsumerConnectionPoint) : typeof(ProviderConnectionPoint);
                if (!connectionPointType.IsSubclassOf(c))
                {
                    return false;
                }
                Type[] types = isConsumer ? ConsumerConnectionPoint.ConstructorTypes : ProviderConnectionPoint.ConstructorTypes;
                if (connectionPointType.GetConstructor(types) == null)
                {
                    return false;
                }
            }
            return true;
        }

        internal static string SerializeType(Type type)
        {
            if (type.Assembly.GlobalAssemblyCache)
            {
                return type.AssemblyQualifiedName;
            }
            return type.FullName;
        }
    }
}

