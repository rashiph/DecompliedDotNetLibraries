namespace System.Configuration
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SettingsPropertyNotFoundException : Exception
    {
        public SettingsPropertyNotFoundException()
        {
        }

        public SettingsPropertyNotFoundException(string message) : base(message)
        {
        }

        protected SettingsPropertyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SettingsPropertyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

