namespace System.Reflection.Cache
{
    using System;

    [Serializable]
    internal enum CacheObjType
    {
        EmptyElement,
        ParameterInfo,
        TypeName,
        RemotingData,
        SerializableAttribute,
        AssemblyName,
        ConstructorInfo,
        FieldType,
        FieldName,
        DefaultMember
    }
}

