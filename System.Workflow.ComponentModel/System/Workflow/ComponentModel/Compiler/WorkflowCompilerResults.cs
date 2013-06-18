namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Runtime;

    [Serializable]
    public sealed class WorkflowCompilerResults : CompilerResults
    {
        private CodeCompileUnit compiledCCU;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WorkflowCompilerResults(TempFileCollection tempFiles) : base(tempFiles)
        {
        }

        internal void AddCompilerErrorsFromCompilerResults(CompilerResults results)
        {
            foreach (CompilerError error in results.Errors)
            {
                base.Errors.Add(new WorkflowCompilerError(error));
            }
            foreach (string str in results.Output)
            {
                base.Output.Add(str);
            }
        }

        public CodeCompileUnit CompiledUnit
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.compiledCCU;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal set
            {
                this.compiledCCU = value;
            }
        }
    }
}

