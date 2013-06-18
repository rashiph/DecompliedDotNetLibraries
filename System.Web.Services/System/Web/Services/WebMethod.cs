namespace System.Web.Services
{
    using System;
    using System.Reflection;

    internal class WebMethod
    {
        internal WebMethodAttribute attribute;
        internal WebServiceBindingAttribute binding;
        internal MethodInfo declaration;

        internal WebMethod(MethodInfo declaration, WebServiceBindingAttribute binding, WebMethodAttribute attribute)
        {
            this.declaration = declaration;
            this.binding = binding;
            this.attribute = attribute;
        }
    }
}

