namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime;

    public class WorkflowCompilerOptionsService : IWorkflowCompilerOptionsService
    {
        internal const string DefaultLanguage = "CSharp";

        public virtual bool CheckTypes
        {
            get
            {
                return false;
            }
        }

        public virtual string Language
        {
            get
            {
                return "CSharp";
            }
        }

        public virtual string RootNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return string.Empty;
            }
        }

        public virtual string TargetFrameworkMoniker
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return string.Empty;
            }
        }
    }
}

