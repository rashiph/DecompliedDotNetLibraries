namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;

    [Serializable]
    internal enum BinaryTypeEnum
    {
        Primitive,
        String,
        Object,
        ObjectUrt,
        ObjectUser,
        ObjectArray,
        StringArray,
        PrimitiveArray
    }
}

