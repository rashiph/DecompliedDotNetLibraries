namespace System.Web.Services.Discovery
{
    using System;

    internal class InvalidDocumentContentsException : Exception
    {
        internal InvalidDocumentContentsException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

