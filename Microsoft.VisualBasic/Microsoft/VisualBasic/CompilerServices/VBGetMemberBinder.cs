namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class VBGetMemberBinder : GetMemberBinder
    {
        private static readonly int _hash = typeof(VBGetMemberBinder).GetHashCode();

        public VBGetMemberBinder(string name) : base(name, true)
        {
        }

        public override bool Equals(object _other)
        {
            VBGetMemberBinder binder = _other as VBGetMemberBinder;
            return ((binder != null) && string.Equals(this.Name, binder.Name));
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (errorSuggestion != null)
            {
                return errorSuggestion;
            }
            return new DynamicMetaObject(Expression.Constant(IDOBinder.missingMemberSentinel), IDOUtils.CreateRestrictions(target, null, null));
        }

        public override int GetHashCode()
        {
            return (_hash ^ this.Name.GetHashCode());
        }
    }
}

