namespace System.Workflow.ComponentModel.Compiler
{
    using System;

    public interface IWorkflowCompilerOptionsService
    {
        bool CheckTypes { get; }

        string Language { get; }

        string RootNamespace { get; }
    }
}

