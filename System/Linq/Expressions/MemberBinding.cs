namespace System.Linq.Expressions
{
    using System;
    using System.Reflection;

    public abstract class MemberBinding
    {
        private MemberInfo _member;
        private MemberBindingType _type;

        [Obsolete("Do not use this constructor. It will be removed in future releases.")]
        protected MemberBinding(MemberBindingType type, MemberInfo member)
        {
            this._type = type;
            this._member = member;
        }

        public override string ToString()
        {
            return ExpressionStringBuilder.MemberBindingToString(this);
        }

        public MemberBindingType BindingType
        {
            get
            {
                return this._type;
            }
        }

        public MemberInfo Member
        {
            get
            {
                return this._member;
            }
        }
    }
}

