namespace System.Configuration
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SettingsPropertyIsReadOnlyException : Exception
    {
        public SettingsPropertyIsReadOnlyException()
        {
        }

        public SettingsPropertyIsReadOnlyException(string message) : base(message)
        {
        }

        protected SettingsPropertyIsReadOnlyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SettingsPropertyIsReadOnlyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

