namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class VBBinaryOperatorBinder : BinaryOperationBinder
    {
        private static readonly int _hash = typeof(VBBinaryOperatorBinder).GetHashCode();
        private readonly Symbols.UserDefinedOperator _Op;

        public VBBinaryOperatorBinder(Symbols.UserDefinedOperator Op, ExpressionType LinqOp) : base(LinqOp)
        {
            this._Op = Op;
        }

        public override bool Equals(object _other)
        {
            VBBinaryOperatorBinder binder = _other as VBBinaryOperatorBinder;
            return (((binder != null) && (this._Op == binder._Op)) && (this.Operation == binder.Operation));
        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            if (IDOUtils.NeedsDeferral(target, null, arg))
            {
                return this.Defer(target, new DynamicMetaObject[] { arg });
            }
            if ((errorSuggestion != null) && (Operators.GetCallableUserDefinedOperator(this._Op, new object[] { target.Value, arg.Value }) == null))
            {
                return errorSuggestion;
            }
            return new DynamicMetaObject(Expression.Call(typeof(Operators).GetMethod("FallbackInvokeUserDefinedOperator"), Expression.Constant(this._Op, typeof(object)), Expression.NewArrayInit(typeof(object), new Expression[] { IDOUtils.ConvertToObject(target.Expression), IDOUtils.ConvertToObject(arg.Expression) })), IDOUtils.CreateRestrictions(target, null, arg));
        }

        public override int GetHashCode()
        {
            return ((_hash ^ this._Op.GetHashCode()) ^ this.Operation.GetHashCode());
        }
    }
}

