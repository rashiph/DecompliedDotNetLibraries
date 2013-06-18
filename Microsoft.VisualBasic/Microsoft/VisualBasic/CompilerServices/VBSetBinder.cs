namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class VBSetBinder : SetMemberBinder
    {
        private static readonly int _hash = typeof(VBSetBinder).GetHashCode();

        public VBSetBinder(string MemberName) : base(MemberName, true)
        {
        }

        public override bool Equals(object _other)
        {
            VBSetBinder binder = _other as VBSetBinder;
            return ((binder != null) && string.Equals(this.Name, binder.Name));
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            if (IDOUtils.NeedsDeferral(target, null, value))
            {
                return this.Defer(target, new DynamicMetaObject[] { value });
            }
            if ((errorSuggestion != null) && !NewLateBinding.CanBindSet(target.Value, this.Name, value.Value, false, false))
            {
                return errorSuggestion;
            }
            Expression expression2 = IDOUtils.ConvertToObject(value.Expression);
            Expression[] initializers = new Expression[] { expression2 };
            return new DynamicMetaObject(Expression.Block(Expression.Call(typeof(NewLateBinding).GetMethod("FallbackSet"), target.Expression, Expression.Constant(this.Name), Expression.NewArrayInit(typeof(object), initializers)), expression2), IDOUtils.CreateRestrictions(target, null, value));
        }

        public override int GetHashCode()
        {
            return (_hash ^ this.Name.GetHashCode());
        }
    }
}

