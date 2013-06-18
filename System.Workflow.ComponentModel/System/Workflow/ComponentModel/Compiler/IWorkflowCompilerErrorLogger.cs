namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("A5367E37-D7AF-4372-8079-D1D6726AEDC8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(false)]
    internal interface IWorkflowCompilerErrorLogger
    {
        void LogError(IWorkflowCompilerError error);
        void LogMessage(string message);
    }
}

