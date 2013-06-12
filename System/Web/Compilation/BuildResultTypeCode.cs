namespace System.Web.Compilation
{
    using System;

    internal enum BuildResultTypeCode
    {
        BuildResultCodeCompileUnit = 7,
        BuildResultCompiledAssembly = 1,
        BuildResultCompiledGlobalAsaxType = 8,
        BuildResultCompiledTemplateType = 3,
        BuildResultCompiledType = 2,
        BuildResultCustomString = 5,
        BuildResultMainCodeAssembly = 6,
        BuildResultResourceAssembly = 9,
        Invalid = -1
    }
}

