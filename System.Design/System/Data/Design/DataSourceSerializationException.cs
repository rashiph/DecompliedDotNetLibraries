namespace System.Data.Design
{
    using System;

    [Serializable]
    internal sealed class DataSourceSerializationException : ApplicationException
    {
        public DataSourceSerializationException(string message) : base(message)
        {
        }
    }
}

