namespace System.Runtime.Serialization
{
    using System;
    using System.Reflection;

    [Serializable]
    internal class MemberHolder
    {
        internal StreamingContext context;
        internal MemberInfo[] members;
        internal Type memberType;

        internal MemberHolder(Type type, StreamingContext ctx)
        {
            this.memberType = type;
            this.context = ctx;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MemberHolder))
            {
                return false;
            }
            MemberHolder holder = (MemberHolder) obj;
            return (object.ReferenceEquals(holder.memberType, this.memberType) && (holder.context.State == this.context.State));
        }

        public override int GetHashCode()
        {
            return this.memberType.GetHashCode();
        }
    }
}

