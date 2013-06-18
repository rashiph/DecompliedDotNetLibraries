namespace System.Web.Services.Protocols
{
    using System;
    using System.Reflection;

    internal class SoapReflectedHeader
    {
        internal bool custom;
        internal SoapHeaderDirection direction;
        internal Type headerType;
        internal MemberInfo memberInfo;
        internal bool repeats;
    }
}

