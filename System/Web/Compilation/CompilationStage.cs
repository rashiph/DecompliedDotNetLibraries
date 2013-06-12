namespace System.Web.Compilation
{
    using System;

    internal enum CompilationStage
    {
        PreTopLevelFiles,
        TopLevelFiles,
        GlobalAsax,
        BrowserCapabilities,
        AfterTopLevelFiles
    }
}

