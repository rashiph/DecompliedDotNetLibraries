namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;

    [Serializable]
    internal enum BinaryHeaderEnum
    {
        SerializedStreamHeader,
        Object,
        ObjectWithMap,
        ObjectWithMapAssemId,
        ObjectWithMapTyped,
        ObjectWithMapTypedAssemId,
        ObjectString,
        Array,
        MemberPrimitiveTyped,
        MemberReference,
        ObjectNull,
        MessageEnd,
        Assembly,
        ObjectNullMultiple256,
        ObjectNullMultiple,
        ArraySinglePrimitive,
        ArraySingleObject,
        ArraySingleString,
        CrossAppDomainMap,
        CrossAppDomainString,
        CrossAppDomainAssembly,
        MethodCall,
        MethodReturn
    }
}

