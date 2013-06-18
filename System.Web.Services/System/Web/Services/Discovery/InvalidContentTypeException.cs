namespace System.Web.Services.Discovery
{
    using System;

    internal class InvalidContentTypeException : Exception
    {
        private string contentType;

        internal InvalidContentTypeException(string message, string contentType) : base(message)
        {
            this.contentType = contentType;
        }

        internal string ContentType
        {
            get
            {
                return this.contentType;
            }
        }
    }
}

