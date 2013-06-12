namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    public sealed class MemberMemberBinding : MemberBinding
    {
        private ReadOnlyCollection<MemberBinding> _bindings;

        internal MemberMemberBinding(MemberInfo member, ReadOnlyCollection<MemberBinding> bindings) : base(MemberBindingType.MemberBinding, member)
        {
            this._bindings = bindings;
        }

        public MemberMemberBinding Update(IEnumerable<MemberBinding> bindings)
        {
            if (bindings == this.Bindings)
            {
                return this;
            }
            return Expression.MemberBind(base.Member, bindings);
        }

        public ReadOnlyCollection<MemberBinding> Bindings
        {
            get
            {
                return this._bindings;
            }
        }
    }
}

