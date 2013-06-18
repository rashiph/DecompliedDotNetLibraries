namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ComPlusListenerInitializationException : Exception
    {
        public ComPlusListenerInitializationException()
        {
        }

        public ComPlusListenerInitializationException(string message) : base(message)
        {
        }

        protected ComPlusListenerInitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ComPlusListenerInitializationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

