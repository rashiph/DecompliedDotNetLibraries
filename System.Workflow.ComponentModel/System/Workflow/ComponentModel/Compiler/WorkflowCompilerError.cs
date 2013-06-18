namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.ComponentModel.Serialization;

    [Serializable]
    public sealed class WorkflowCompilerError : CompilerError, IWorkflowCompilerError
    {
        private bool incrementLineAndColumn;
        private string propertyName;
        private Hashtable userData;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowCompilerError()
        {
        }

        internal WorkflowCompilerError(CompilerError error)
        {
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }
            base.Column = error.Column - 1;
            base.ErrorNumber = error.ErrorNumber;
            base.ErrorText = error.ErrorText;
            base.FileName = error.FileName;
            base.IsWarning = error.IsWarning;
            base.Line = error.Line - 1;
            this.incrementLineAndColumn = true;
        }

        public WorkflowCompilerError(string fileName, WorkflowMarkupSerializationException exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            base.FileName = fileName;
            base.Line = exception.LineNumber - 1;
            base.Column = exception.LinePosition - 1;
            base.ErrorText = exception.Message;
            base.ErrorNumber = 0x15b.ToString(CultureInfo.InvariantCulture);
            this.incrementLineAndColumn = true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowCompilerError(string fileName, int line, int column, string errorNumber, string errorText) : base(fileName, line, column, errorNumber, errorText)
        {
        }

        public override string ToString()
        {
            if (base.FileName.Length > 0)
            {
                if ((base.Line <= 0) || (base.Column <= 0))
                {
                    return string.Format(CultureInfo.CurrentCulture, "{0} : {1} {2}: {3}", new object[] { base.FileName, base.IsWarning ? "warning" : "error", base.ErrorNumber, base.ErrorText });
                }
                return string.Format(CultureInfo.CurrentCulture, "{0}({1},{2}) : {3} {4}: {5}", new object[] { base.FileName, this.incrementLineAndColumn ? (base.Line + 1) : base.Line, this.incrementLineAndColumn ? (base.Column + 1) : base.Column, base.IsWarning ? "warning" : "error", base.ErrorNumber, base.ErrorText });
            }
            return string.Format(CultureInfo.CurrentCulture, "{0} {1}: {2}", new object[] { base.IsWarning ? "warning" : "error", base.ErrorNumber, base.ErrorText });
        }

        public string PropertyName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propertyName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.propertyName = value;
            }
        }

        int IWorkflowCompilerError.ColumnNumber
        {
            get
            {
                return base.Column;
            }
        }

        string IWorkflowCompilerError.Document
        {
            get
            {
                return base.FileName;
            }
        }

        string IWorkflowCompilerError.ErrorNumber
        {
            get
            {
                return base.ErrorNumber;
            }
        }

        bool IWorkflowCompilerError.IsWarning
        {
            get
            {
                return base.IsWarning;
            }
        }

        int IWorkflowCompilerError.LineNumber
        {
            get
            {
                return base.Line;
            }
        }

        string IWorkflowCompilerError.Text
        {
            get
            {
                return base.ErrorText;
            }
        }

        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                {
                    this.userData = new Hashtable();
                }
                return this.userData;
            }
        }
    }
}

