namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, ComVisible(false), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("AEA0CDAE-ADB5-46c6-A5ED-DBD516B3E0C1")]
    internal interface IWorkflowCompilerError
    {
        string Document { get; }
        bool IsWarning { get; }
        string Text { get; }
        string ErrorNumber { get; }
        int LineNumber { get; }
        int ColumnNumber { get; }
    }
}

