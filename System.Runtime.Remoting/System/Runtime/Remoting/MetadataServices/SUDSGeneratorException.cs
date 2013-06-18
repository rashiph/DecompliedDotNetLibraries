namespace System.Runtime.Remoting.MetadataServices
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class SUDSGeneratorException : Exception
    {
        internal SUDSGeneratorException(string msg) : base(msg)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected SUDSGeneratorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

