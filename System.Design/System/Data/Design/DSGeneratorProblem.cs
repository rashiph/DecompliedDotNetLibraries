namespace System.Data.Design
{
    using System;

    internal sealed class DSGeneratorProblem
    {
        private string message;
        private DataSourceComponent problemSource;
        private ProblemSeverity severity;

        internal DSGeneratorProblem(string message, ProblemSeverity severity, DataSourceComponent problemSource)
        {
            this.message = message;
            this.severity = severity;
            this.problemSource = problemSource;
        }

        internal string Message
        {
            get
            {
                return this.message;
            }
        }

        internal DataSourceComponent ProblemSource
        {
            get
            {
                return this.problemSource;
            }
        }

        internal ProblemSeverity Severity
        {
            get
            {
                return this.severity;
            }
        }
    }
}

