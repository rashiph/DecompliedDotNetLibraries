namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class VBIndexSetComplexBinder : SetIndexBinder
    {
        private static readonly int _hash = typeof(VBIndexSetComplexBinder).GetHashCode();
        private readonly bool _optimisticSet;
        private readonly bool _rValueBase;

        public VBIndexSetComplexBinder(CallInfo CallInfo, bool OptimisticSet, bool RValueBase) : base(CallInfo)
        {
            this._optimisticSet = OptimisticSet;
            this._rValueBase = RValueBase;
        }

        public override bool Equals(object _other)
        {
            VBIndexSetComplexBinder binder = _other as VBIndexSetComplexBinder;
            return (((binder != null) && this.CallInfo.Equals(binder.CallInfo)) && ((this._optimisticSet == binder._optimisticSet) && (this._rValueBase == binder._rValueBase)));
        }

        public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] packedIndexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            if (IDOUtils.NeedsDeferral(target, packedIndexes, value))
            {
                Array.Resize<DynamicMetaObject>(ref packedIndexes, packedIndexes.Length + 1);
                packedIndexes[packedIndexes.Length - 1] = value;
                return this.Defer(target, packedIndexes);
            }
            string[] argNames = null;
            Expression[] args = null;
            object[] argValues = null;
            IDOUtils.UnpackArguments(packedIndexes, this.CallInfo, ref args, ref argNames, ref argValues);
            object[] array = new object[argValues.Length + 1];
            argValues.CopyTo(array, 0);
            array[argValues.Length] = value.Value;
            if ((errorSuggestion != null) && !NewLateBinding.CanIndexSetComplex(target.Value, array, argNames, this._optimisticSet, this._rValueBase))
            {
                return errorSuggestion;
            }
            Expression expression2 = IDOUtils.ConvertToObject(value.Expression);
            Expression[] expressionArray2 = new Expression[args.Length + 1];
            args.CopyTo(expressionArray2, 0);
            expressionArray2[args.Length] = expression2;
            return new DynamicMetaObject(Expression.Block(Expression.Call(typeof(NewLateBinding).GetMethod("FallbackIndexSetComplex"), target.Expression, Expression.NewArrayInit(typeof(object), expressionArray2), Expression.Constant(argNames, typeof(string[])), Expression.Constant(this._optimisticSet), Expression.Constant(this._rValueBase)), expression2), IDOUtils.CreateRestrictions(target, packedIndexes, value));
        }

        public override int GetHashCode()
        {
            return (((_hash ^ this.CallInfo.GetHashCode()) ^ this._optimisticSet.GetHashCode()) ^ this._rValueBase.GetHashCode());
        }
    }
}

