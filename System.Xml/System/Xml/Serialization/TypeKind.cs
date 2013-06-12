namespace System.Xml.Serialization
{
    using System;

    internal enum TypeKind
    {
        Root,
        Primitive,
        Enum,
        Struct,
        Class,
        Array,
        Collection,
        Enumerable,
        Void,
        Node,
        Attribute,
        Serializable
    }
}

