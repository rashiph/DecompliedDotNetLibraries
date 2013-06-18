namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class WorkflowMarkupSourceAttribute : Attribute
    {
        private string fileName;
        private string md5Digest;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowMarkupSourceAttribute(string fileName, string md5Digest)
        {
            this.fileName = fileName;
            this.md5Digest = md5Digest;
        }

        public string FileName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.fileName;
            }
        }

        public string MD5Digest
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.md5Digest;
            }
        }
    }
}

