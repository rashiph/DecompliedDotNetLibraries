namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal interface ITypeCacheManager
    {
        void FindOrCreateType(Guid riid, out Type interfaceType, bool noAssemblyGeneration, bool isServer);
        void FindOrCreateType(Guid typeLibId, string typeLibVersion, Guid typeDefId, out Type userDefinedType, bool noAssemblyGeneration);
        void FindOrCreateType(Type serverType, Guid riid, out Type interfaceType, bool noAssemblyGeneration, bool isServer);
        Assembly ResolveAssembly(Guid assembly);
    }
}

