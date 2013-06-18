namespace Microsoft.Compiler.VisualBasic
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal sealed class CompilerResults
    {
        private LambdaExpression m_codeBlock = null;
        private List<Microsoft.Compiler.VisualBasic.Error> m_errors = new List<Microsoft.Compiler.VisualBasic.Error>();
        private List<Warning> m_warnings = new List<Warning>();

        internal void AddError(int errorCode, string description, SourceLocation sourceLocation)
        {
            Microsoft.Compiler.VisualBasic.Error item = new Microsoft.Compiler.VisualBasic.Error(errorCode, description, sourceLocation);
            this.m_errors.Add(item);
        }

        internal void AddWarning(int warningCode, string description, SourceLocation sourceLocation)
        {
            Warning item = new Warning(warningCode, description, sourceLocation);
            this.m_warnings.Add(item);
        }

        internal void SetCodeBlock(LambdaExpression value)
        {
            this.m_codeBlock = value;
        }

        public LambdaExpression CodeBlock
        {
            get
            {
                return this.m_codeBlock;
            }
        }

        public IList<Microsoft.Compiler.VisualBasic.Error> Errors
        {
            get
            {
                return this.m_errors;
            }
        }

        public IList<Warning> Warnings
        {
            get
            {
                return this.m_warnings;
            }
        }
    }
}

