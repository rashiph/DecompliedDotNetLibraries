namespace System.Activities.Expressions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class LambdaSerializationException : Exception
    {
        public LambdaSerializationException() : base(System.Activities.SR.LambdaNotXamlSerializable)
        {
        }

        public LambdaSerializationException(string message) : base(message)
        {
        }

        protected LambdaSerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public LambdaSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

