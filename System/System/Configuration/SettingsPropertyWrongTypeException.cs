namespace System.Configuration
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SettingsPropertyWrongTypeException : Exception
    {
        public SettingsPropertyWrongTypeException()
        {
        }

        public SettingsPropertyWrongTypeException(string message) : base(message)
        {
        }

        protected SettingsPropertyWrongTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SettingsPropertyWrongTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

