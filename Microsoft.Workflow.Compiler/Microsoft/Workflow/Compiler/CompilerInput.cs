namespace Microsoft.Workflow.Compiler
{
    using System;
    using System.Runtime.Serialization;
    using System.Workflow.ComponentModel.Compiler;

    [DataContract]
    internal class CompilerInput
    {
        [DataMember]
        private readonly string[] files;
        [DataMember]
        private readonly WorkflowCompilerParameters parameters;

        public CompilerInput(WorkflowCompilerParameters parameters, string[] files)
        {
            this.parameters = parameters;
            this.files = files;
        }

        public string[] Files
        {
            get
            {
                return this.files;
            }
        }

        public WorkflowCompilerParameters Parameters
        {
            get
            {
                return this.parameters;
            }
        }
    }
}

