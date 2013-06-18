namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class AppConfigException : ApplicationException
    {
        private int column;
        private string fileName;
        private int line;

        protected AppConfigException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.fileName = string.Empty;
        }

        public AppConfigException(string message, string fileName, int line, int column, Exception inner) : base(message, inner)
        {
            this.fileName = string.Empty;
            this.fileName = fileName;
            this.line = line;
            this.column = column;
        }

        internal int Column
        {
            get
            {
                return this.column;
            }
        }

        internal string FileName
        {
            get
            {
                return this.fileName;
            }
        }

        internal int Line
        {
            get
            {
                return this.line;
            }
        }
    }
}

