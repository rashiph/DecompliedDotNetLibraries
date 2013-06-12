namespace System.ComponentModel
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class InvalidEnumArgumentException : ArgumentException
    {
        public InvalidEnumArgumentException() : this(null)
        {
        }

        public InvalidEnumArgumentException(string message) : base(message)
        {
        }

        protected InvalidEnumArgumentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidEnumArgumentException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidEnumArgumentException(string argumentName, int invalidValue, Type enumClass) : base(SR.GetString("InvalidEnumArgument", new object[] { argumentName, invalidValue.ToString(CultureInfo.CurrentCulture), enumClass.Name }), argumentName)
        {
        }
    }
}

