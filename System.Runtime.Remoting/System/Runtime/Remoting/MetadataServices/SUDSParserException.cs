namespace System.Runtime.Remoting.MetadataServices
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class SUDSParserException : Exception
    {
        internal SUDSParserException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected SUDSParserException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

