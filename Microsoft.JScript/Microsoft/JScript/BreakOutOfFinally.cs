namespace Microsoft.JScript
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class BreakOutOfFinally : ApplicationException
    {
        public int target;

        public BreakOutOfFinally(int target)
        {
            this.target = target;
        }

        public BreakOutOfFinally(string m) : base(m)
        {
        }

        private BreakOutOfFinally(SerializationInfo s, StreamingContext c) : base(s, c)
        {
            this.target = s.GetInt32("Target");
        }

        public BreakOutOfFinally(string m, Exception e) : base(m, e)
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

