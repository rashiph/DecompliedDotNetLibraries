namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Reflection;

    internal sealed class SerObjectInfoCache
    {
        internal string assemblyString;
        internal string fullTypeName;
        internal bool hasTypeForwardedFrom;
        internal MemberInfo[] memberInfos;
        internal string[] memberNames;
        internal Type[] memberTypes;

        internal SerObjectInfoCache(Type type)
        {
            TypeInformation typeInformation = BinaryFormatter.GetTypeInformation(type);
            this.fullTypeName = typeInformation.FullTypeName;
            this.assemblyString = typeInformation.AssemblyString;
            this.hasTypeForwardedFrom = typeInformation.HasTypeForwardedFrom;
        }

        internal SerObjectInfoCache(string typeName, string assemblyName, bool hasTypeForwardedFrom)
        {
            this.fullTypeName = typeName;
            this.assemblyString = assemblyName;
            this.hasTypeForwardedFrom = hasTypeForwardedFrom;
        }
    }
}

