namespace System.Web.Services
{
    using System;
    using System.Web.Services.Protocols;

    internal class WebServiceReflector
    {
        private WebServiceReflector()
        {
        }

        internal static WebServiceAttribute GetAttribute(Type type)
        {
            object[] customAttributes = type.GetCustomAttributes(typeof(WebServiceAttribute), false);
            if (customAttributes.Length == 0)
            {
                return new WebServiceAttribute();
            }
            return (WebServiceAttribute) customAttributes[0];
        }

        internal static WebServiceAttribute GetAttribute(LogicalMethodInfo[] methodInfos)
        {
            if (methodInfos.Length == 0)
            {
                return new WebServiceAttribute();
            }
            return GetAttribute(GetMostDerivedType(methodInfos));
        }

        internal static Type GetMostDerivedType(LogicalMethodInfo[] methodInfos)
        {
            if (methodInfos.Length == 0)
            {
                return null;
            }
            Type declaringType = methodInfos[0].DeclaringType;
            for (int i = 1; i < methodInfos.Length; i++)
            {
                Type type2 = methodInfos[i].DeclaringType;
                if (type2.IsSubclassOf(declaringType))
                {
                    declaringType = type2;
                }
            }
            return declaringType;
        }
    }
}

