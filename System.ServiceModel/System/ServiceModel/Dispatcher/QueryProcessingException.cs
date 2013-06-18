namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml.XPath;

    internal class QueryProcessingException : XPathException
    {
        private QueryProcessingError error;

        internal QueryProcessingException(QueryProcessingError error) : this(error, null)
        {
            this.error = error;
        }

        internal QueryProcessingException(QueryProcessingError error, string message) : base(message, (Exception) null)
        {
            this.error = error;
        }

        public override string ToString()
        {
            return this.error.ToString();
        }
    }
}

