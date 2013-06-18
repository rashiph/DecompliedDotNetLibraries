namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class VBInvokeBinder : InvokeBinder
    {
        private static readonly int _hash = typeof(VBGetBinder).GetHashCode();
        private readonly bool _lateCall;

        public VBInvokeBinder(CallInfo CallInfo, bool LateCall) : base(CallInfo)
        {
            this._lateCall = LateCall;
        }

        public override bool Equals(object _other)
        {
            VBInvokeBinder binder = _other as VBInvokeBinder;
            return (((binder != null) && this.CallInfo.Equals(binder.CallInfo)) && this._lateCall.Equals(binder._lateCall));
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] packedArgs, DynamicMetaObject errorSuggestion)
        {
            if (IDOUtils.NeedsDeferral(target, packedArgs, null))
            {
                return this.Defer(target, packedArgs);
            }
            Expression[] args = null;
            string[] argNames = null;
            object[] argValues = null;
            IDOUtils.UnpackArguments(packedArgs, this.CallInfo, ref args, ref argNames, ref argValues);
            if ((errorSuggestion != null) && !NewLateBinding.CanBindInvokeDefault(target.Value, argValues, argNames, this._lateCall))
            {
                return errorSuggestion;
            }
            ParameterExpression left = Expression.Variable(typeof(object), "result");
            ParameterExpression expression = Expression.Variable(typeof(object[]), "array");
            Expression right = Expression.Call(typeof(NewLateBinding).GetMethod(this._lateCall ? "LateCallInvokeDefault" : "LateGetInvokeDefault"), target.Expression, Expression.Assign(expression, Expression.NewArrayInit(typeof(object), args)), Expression.Constant(argNames, typeof(string[])), Expression.Constant(this._lateCall));
            return new DynamicMetaObject(Expression.Block(new ParameterExpression[] { left, expression }, new Expression[] { Expression.Assign(left, right), IDOUtils.GetWriteBack(args, expression), left }), IDOUtils.CreateRestrictions(target, packedArgs, null));
        }

        public override int GetHashCode()
        {
            return ((_hash ^ this.CallInfo.GetHashCode()) ^ this._lateCall.GetHashCode());
        }
    }
}

