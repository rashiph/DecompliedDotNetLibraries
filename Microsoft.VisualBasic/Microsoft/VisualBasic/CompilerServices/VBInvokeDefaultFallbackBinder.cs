namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class VBInvokeDefaultFallbackBinder : GetIndexBinder
    {
        private static readonly int _hash = typeof(VBInvokeDefaultFallbackBinder).GetHashCode();
        private readonly bool _reportErrors;

        public VBInvokeDefaultFallbackBinder(CallInfo CallInfo, bool ReportErrors) : base(CallInfo)
        {
            this._reportErrors = ReportErrors;
        }

        public override bool Equals(object _other)
        {
            VBInvokeDefaultFallbackBinder binder = _other as VBInvokeDefaultFallbackBinder;
            return (((binder != null) && this.CallInfo.Equals(binder.CallInfo)) && (this._reportErrors == binder._reportErrors));
        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] packedArgs, DynamicMetaObject errorSuggestion)
        {
            if (IDOUtils.NeedsDeferral(target, packedArgs, null))
            {
                return this.Defer(target, packedArgs);
            }
            Expression[] args = null;
            string[] argNames = null;
            object[] argValues = null;
            IDOUtils.UnpackArguments(packedArgs, this.CallInfo, ref args, ref argNames, ref argValues);
            if ((errorSuggestion != null) && !NewLateBinding.CanBindInvokeDefault(target.Value, argValues, argNames, this._reportErrors))
            {
                return errorSuggestion;
            }
            ParameterExpression left = Expression.Variable(typeof(object), "result");
            ParameterExpression expression = Expression.Variable(typeof(object[]), "array");
            Expression right = Expression.Call(typeof(NewLateBinding).GetMethod("FallbackInvokeDefault2"), target.Expression, Expression.Assign(expression, Expression.NewArrayInit(typeof(object), args)), Expression.Constant(argNames, typeof(string[])), Expression.Constant(this._reportErrors));
            return new DynamicMetaObject(Expression.Block(new ParameterExpression[] { left, expression }, new Expression[] { Expression.Assign(left, right), IDOUtils.GetWriteBack(args, expression), left }), IDOUtils.CreateRestrictions(target, packedArgs, null));
        }

        public override int GetHashCode()
        {
            return ((_hash ^ this.CallInfo.GetHashCode()) ^ this._reportErrors.GetHashCode());
        }
    }
}

