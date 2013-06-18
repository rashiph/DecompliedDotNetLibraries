namespace Microsoft.JScript
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class ContinueOutOfFinally : ApplicationException
    {
        public int target;

        public ContinueOutOfFinally() : this(0)
        {
        }

        public ContinueOutOfFinally(int target)
        {
            this.target = target;
        }

        public ContinueOutOfFinally(string m) : base(m)
        {
        }

        private ContinueOutOfFinally(SerializationInfo s, StreamingContext c) : base(s, c)
        {
            this.target = s.GetInt32("Target");
        }

        public ContinueOutOfFinally(string m, Exception e) : base(m, e)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo s, StreamingContext c)
        {
            base.GetObjectData(s, c);
            s.AddValue("Target", this.target);
        }
    }
}

