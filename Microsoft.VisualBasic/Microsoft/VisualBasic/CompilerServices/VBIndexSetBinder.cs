namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class VBIndexSetBinder : SetIndexBinder
    {
        private static readonly int _hash = typeof(VBIndexSetBinder).GetHashCode();

        public VBIndexSetBinder(CallInfo CallInfo) : base(CallInfo)
        {
        }

        public override bool Equals(object _other)
        {
            VBIndexSetBinder binder = _other as VBIndexSetBinder;
            return ((binder != null) && this.CallInfo.Equals(binder.CallInfo));
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
            if ((errorSuggestion != null) && !NewLateBinding.CanIndexSetComplex(target.Value, array, argNames, false, false))
            {
                return errorSuggestion;
            }
            Expression expression2 = IDOUtils.ConvertToObject(value.Expression);
            Expression[] expressionArray2 = new Expression[args.Length + 1];
            args.CopyTo(expressionArray2, 0);
            expressionArray2[args.Length] = expression2;
            return new DynamicMetaObject(Expression.Block(Expression.Call(typeof(NewLateBinding).GetMethod("FallbackIndexSet"), target.Expression, Expression.NewArrayInit(typeof(object), expressionArray2), Expression.Constant(argNames, typeof(string[]))), expression2), IDOUtils.CreateRestrictions(target, packedIndexes, value));
        }

        public override int GetHashCode()
        {
            return (_hash ^ this.CallInfo.GetHashCode());
        }
    }
}

