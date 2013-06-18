namespace System.Web.Services.Description
{
    using System;

    internal enum CodeFlags
    {
        IsAbstract = 2,
        IsByRef = 0x10,
        IsInterface = 0x40,
        IsNew = 8,
        IsOut = 0x20,
        IsPublic = 1,
        IsStruct = 4
    }
}

