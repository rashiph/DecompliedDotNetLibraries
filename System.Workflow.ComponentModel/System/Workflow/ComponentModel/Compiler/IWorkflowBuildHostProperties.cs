namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("8AA9644E-1F6A-4F4C-83E3-D0BAD4B2BB21")]
    internal interface IWorkflowBuildHostProperties
    {
        bool SkipWorkflowCompilation { get; set; }
    }
}

