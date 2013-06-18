namespace Microsoft.JScript
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class ReturnOutOfFinally : ApplicationException
    {
        public ReturnOutOfFinally()
        {
        }

        public ReturnOutOfFinally(string m) : base(m)
        {
        }

        private ReturnOutOfFinally(SerializationInfo s, StreamingContext c) : base(s, c)
        {
        }

        public ReturnOutOfFinally(string m, Exception e) : base(m, e)
        {
        }
    }
}

