namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Flags]
    public enum CodeTypeReferenceOptions
    {
        GenericTypeParameter = 2,
        GlobalReference = 1
    }
}

