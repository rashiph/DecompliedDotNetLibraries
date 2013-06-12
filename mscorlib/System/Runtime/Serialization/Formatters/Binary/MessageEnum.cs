namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;

    [Serializable, Flags]
    internal enum MessageEnum
    {
        ArgsInArray = 8,
        ArgsInline = 2,
        ArgsIsArray = 4,
        ContextInArray = 0x40,
        ContextInline = 0x20,
        ExceptionInArray = 0x2000,
        GenericMethod = 0x8000,
        MethodSignatureInArray = 0x80,
        NoArgs = 1,
        NoContext = 0x10,
        NoReturnValue = 0x200,
        PropertyInArray = 0x100,
        ReturnValueInArray = 0x1000,
        ReturnValueInline = 0x800,
        ReturnValueVoid = 0x400
    }
}

