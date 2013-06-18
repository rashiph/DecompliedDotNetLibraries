namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class BerConversionException : DirectoryException
    {
        public BerConversionException() : base(Res.GetString("BerConversionError"))
        {
        }

        public BerConversionException(string message) : base(message)
        {
        }

        protected BerConversionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BerConversionException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

