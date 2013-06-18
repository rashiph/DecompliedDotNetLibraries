namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;

    [Flags, Guid("BA5ED019-F669-3C35-93AC-3ABF776B62B3"), ComVisible(true)]
    public enum JSFunctionAttributeEnum
    {
        ClassicFunction = 0x23,
        ClassicNestedFunction = 0x2f,
        HasArguments = 1,
        HasEngine = 0x20,
        HasStackFrame = 8,
        HasThisObject = 2,
        HasVarArgs = 0x10,
        IsExpandoMethod = 0x40,
        IsInstanceNestedClassConstructor = 0x80,
        IsNested = 4,
        NestedFunction = 0x2c,
        None = 0
    }
}

