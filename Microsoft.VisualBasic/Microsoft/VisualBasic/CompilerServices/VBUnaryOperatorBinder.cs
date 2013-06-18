namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class VBUnaryOperatorBinder : UnaryOperationBinder
    {
        private static readonly int _hash = typeof(VBUnaryOperatorBinder).GetHashCode();
        private readonly Symbols.UserDefinedOperator _Op;

        public VBUnaryOperatorBinder(Symbols.UserDefinedOperator Op, ExpressionType LinqOp) : base(LinqOp)
        {
            this._Op = Op;
        }

        public override bool Equals(object _other)
        {
            VBUnaryOperatorBinder binder = _other as VBUnaryOperatorBinder;
            return (((binder != null) && (this._Op == binder._Op)) && (this.Operation == binder.Operation));
        }

        public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (IDOUtils.NeedsDeferral(target, null, null))
            {
                return this.Defer(target, new DynamicMetaObject[0]);
            }
            if ((errorSuggestion != null) && (Operators.GetCallableUserDefinedOperator(this._Op, new object[] { target.Value }) == null))
            {
                return errorSuggestion;
            }
            return new DynamicMetaObject(Expression.Call(typeof(Operators).GetMethod("FallbackInvokeUserDefinedOperator"), Expression.Constant(this._Op, typeof(object)), Expression.NewArrayInit(typeof(object), new Expression[] { IDOUtils.ConvertToObject(target.Expression) })), IDOUtils.CreateRestrictions(target, null, null));
        }

        public override int GetHashCode()
        {
            return ((_hash ^ this._Op.GetHashCode()) ^ this.Operation.GetHashCode());
        }
    }
}

