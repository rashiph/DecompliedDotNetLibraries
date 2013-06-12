namespace System
{
    using System.Runtime.Serialization;

    [Serializable]
    public class TypeAccessException : TypeLoadException
    {
        public TypeAccessException() : base(Environment.GetResourceString("Arg_TypeAccessException"))
        {
            base.SetErrorCode(-2146233021);
        }

        public TypeAccessException(string message) : base(message)
        {
            base.SetErrorCode(-2146233021);
        }

        protected TypeAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            base.SetErrorCode(-2146233021);
        }

        public TypeAccessException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233021);
        }
    }
}

