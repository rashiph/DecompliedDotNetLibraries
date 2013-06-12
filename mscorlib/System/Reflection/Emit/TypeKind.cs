namespace System.Reflection.Emit
{
    using System;

    [Serializable]
    internal enum TypeKind
    {
        IsArray = 1,
        IsByRef = 3,
        IsPointer = 2
    }
}

