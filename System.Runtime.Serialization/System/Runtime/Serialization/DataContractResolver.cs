namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Xml;

    public abstract class DataContractResolver
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected DataContractResolver()
        {
        }

        public abstract Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver);
        public abstract bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace);
    }
}

