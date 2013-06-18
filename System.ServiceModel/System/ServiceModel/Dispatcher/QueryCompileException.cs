namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml.XPath;

    internal class QueryCompileException : XPathException
    {
        private QueryCompileError error;

        internal QueryCompileException(QueryCompileError error) : this(error, null)
        {
            this.error = error;
        }

        internal QueryCompileException(QueryCompileError error, string message) : base(message, (Exception) null)
        {
            this.error = error;
        }

        public override string ToString()
        {
            return this.error.ToString();
        }
    }
}

