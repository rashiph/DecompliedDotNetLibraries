namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    public sealed class MemberListBinding : MemberBinding
    {
        private ReadOnlyCollection<ElementInit> _initializers;

        internal MemberListBinding(MemberInfo member, ReadOnlyCollection<ElementInit> initializers) : base(MemberBindingType.ListBinding, member)
        {
            this._initializers = initializers;
        }

        public MemberListBinding Update(IEnumerable<ElementInit> initializers)
        {
            if (initializers == this.Initializers)
            {
                return this;
            }
            return Expression.ListBind(base.Member, initializers);
        }

        public ReadOnlyCollection<ElementInit> Initializers
        {
            get
            {
                return this._initializers;
            }
        }
    }
}

