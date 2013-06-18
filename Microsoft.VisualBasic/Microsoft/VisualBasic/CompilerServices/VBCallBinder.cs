namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class VBCallBinder : InvokeMemberBinder
    {
        private static readonly int _hash = typeof(VBCallBinder).GetHashCode();
        private readonly bool _ignoreReturn;

        public VBCallBinder(string MemberName, CallInfo CallInfo, bool IgnoreReturn) : base(MemberName, true, CallInfo)
        {
            this._ignoreReturn = IgnoreReturn;
        }

        public override bool Equals(object _other)
        {
            VBCallBinder binder = _other as VBCallBinder;
            return (((binder != null) && string.Equals(this.Name, binder.Name)) && (this.CallInfo.Equals(binder.CallInfo) && (this._ignoreReturn == binder._ignoreReturn)));
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] packedArgs, DynamicMetaObject errorSuggestion)
        {
            return new VBInvokeBinder(this.CallInfo, true).FallbackInvoke(target, packedArgs, errorSuggestion);
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] packedArgs, DynamicMetaObject errorSuggestion)
        {
            if (IDOUtils.NeedsDeferral(target, packedArgs, null))
            {
                return this.Defer(target, packedArgs);
            }
            Expression[] args = null;
            string[] argNames = null;
            object[] argValues = null;
            IDOUtils.UnpackArguments(packedArgs, this.CallInfo, ref args, ref argNames, ref argValues);
            if ((errorSuggestion != null) && !NewLateBinding.CanBindCall(target.Value, this.Name, argValues, argNames, this._ignoreReturn))
            {
                return errorSuggestion;
            }
            ParameterExpression left = Expression.Variable(typeof(object), "result");
            ParameterExpression expression = Expression.Variable(typeof(object[]), "array");
            Expression right = Expression.Call(typeof(NewLateBinding).GetMethod("FallbackCall"), target.Expression, Expression.Constant(this.Name, typeof(string)), Expression.Assign(expression, Expression.NewArrayInit(typeof(object), args)), Expression.Constant(argNames, typeof(string[])), Expression.Constant(this._ignoreReturn, typeof(bool)));
            return new DynamicMetaObject(Expression.Block(new ParameterExpression[] { left, expression }, new Expression[] { Expression.Assign(left, right), IDOUtils.GetWriteBack(args, expression), left }), IDOUtils.CreateRestrictions(target, packedArgs, null));
        }

        public override int GetHashCode()
        {
            return (((_hash ^ this.Name.GetHashCode()) ^ this.CallInfo.GetHashCode()) ^ this._ignoreReturn.GetHashCode());
        }
    }
}

