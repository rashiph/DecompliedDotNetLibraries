namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Reflection;

    internal sealed class SerObjectInfoCache
    {
        internal string assemblyString;
        internal string fullTypeName;
        internal SoapAttributeInfo[] memberAttributeInfos;
        internal MemberInfo[] memberInfos;
        internal string[] memberNames;
        internal Type[] memberTypes;
    }
}

