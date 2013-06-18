namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class CardSpaceException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CardSpaceException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CardSpaceException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CardSpaceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CardSpaceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

