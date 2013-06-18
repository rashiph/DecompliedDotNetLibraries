namespace System.Web.Services
{
    using System;
    using System.Web.Services.Protocols;

    internal class WebServiceBindingReflector
    {
        private WebServiceBindingReflector()
        {
        }

        internal static WebServiceBindingAttribute GetAttribute(Type type)
        {
            while (type != null)
            {
                object[] customAttributes = type.GetCustomAttributes(typeof(WebServiceBindingAttribute), false);
                if (customAttributes.Length != 0)
                {
                    if (customAttributes.Length > 1)
                    {
                        throw new ArgumentException(Res.GetString("OnlyOneWebServiceBindingAttributeMayBeSpecified1", new object[] { type.FullName }), "type");
                    }
                    return (WebServiceBindingAttribute) customAttributes[0];
                }
                type = type.BaseType;
            }
            return null;
        }

        internal static WebServiceBindingAttribute GetAttribute(LogicalMethodInfo methodInfo, string binding)
        {
            if (methodInfo.Binding != null)
            {
                if ((binding.Length > 0) && (methodInfo.Binding.Name != binding))
                {
                    throw new InvalidOperationException(Res.GetString("WebInvalidBindingName", new object[] { binding, methodInfo.Binding.Name }));
                }
                return methodInfo.Binding;
            }
            Type declaringType = methodInfo.DeclaringType;
            object[] customAttributes = declaringType.GetCustomAttributes(typeof(WebServiceBindingAttribute), false);
            WebServiceBindingAttribute attribute = null;
            foreach (WebServiceBindingAttribute attribute2 in customAttributes)
            {
                if (attribute2.Name == binding)
                {
                    if (attribute != null)
                    {
                        throw new ArgumentException(Res.GetString("MultipleBindingsWithSameName2", new object[] { declaringType.FullName, binding, "methodInfo" }));
                    }
                    attribute = attribute2;
                }
            }
            if (((attribute == null) && (binding != null)) && (binding.Length > 0))
            {
                throw new ArgumentException(Res.GetString("TypeIsMissingWebServiceBindingAttributeThat2", new object[] { declaringType.FullName, binding }), "methodInfo");
            }
            return attribute;
        }
    }
}

