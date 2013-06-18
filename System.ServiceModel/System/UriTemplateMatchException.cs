namespace System
{
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [Serializable, TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class UriTemplateMatchException : SystemException
    {
        public UriTemplateMatchException()
        {
        }

        public UriTemplateMatchException(string message) : base(message)
        {
        }

        protected UriTemplateMatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public UriTemplateMatchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

