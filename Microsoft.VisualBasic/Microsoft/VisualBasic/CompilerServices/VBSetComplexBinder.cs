namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class VBSetComplexBinder : SetMemberBinder
    {
        private static readonly int _hash = typeof(VBSetComplexBinder).GetHashCode();
        private readonly bool _optimisticSet;
        private readonly bool _rValueBase;

        public VBSetComplexBinder(string MemberName, bool OptimisticSet, bool RValueBase) : base(MemberName, true)
        {
            this._optimisticSet = OptimisticSet;
            this._rValueBase = RValueBase;
        }

        public override bool Equals(object _other)
        {
            VBSetComplexBinder binder = _other as VBSetComplexBinder;
            return (((binder != null) && string.Equals(this.Name, binder.Name)) && ((this._optimisticSet == binder._optimisticSet) && (this._rValueBase == binder._rValueBase)));
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            if (IDOUtils.NeedsDeferral(target, null, value))
            {
                return this.Defer(target, new DynamicMetaObject[] { value });
            }
            if ((errorSuggestion != null) && !NewLateBinding.CanBindSet(target.Value, this.Name, value.Value, this._optimisticSet, this._rValueBase))
            {
                return errorSuggestion;
            }
            Expression expression2 = IDOUtils.ConvertToObject(value.Expression);
            Expression[] initializers = new Expression[] { expression2 };
            return new DynamicMetaObject(Expression.Block(Expression.Call(typeof(NewLateBinding).GetMethod("FallbackSetComplex"), target.Expression, Expression.Constant(this.Name), Expression.NewArrayInit(typeof(object), initializers), Expression.Constant(this._optimisticSet), Expression.Constant(this._rValueBase)), expression2), IDOUtils.CreateRestrictions(target, null, value));
        }

        public override int GetHashCode()
        {
            return (((_hash ^ this.Name.GetHashCode()) ^ this._optimisticSet.GetHashCode()) ^ this._rValueBase.GetHashCode());
        }
    }
}

