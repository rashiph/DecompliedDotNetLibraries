namespace System.Data.Design
{
    using System;

    [Serializable]
    internal sealed class NameValidationException : ApplicationException
    {
        public NameValidationException(string message) : base(message)
        {
        }
    }
}

