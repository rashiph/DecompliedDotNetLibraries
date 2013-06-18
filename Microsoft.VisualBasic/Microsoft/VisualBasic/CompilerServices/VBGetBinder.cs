namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class VBGetBinder : InvokeMemberBinder
    {
        private static readonly int _hash = typeof(VBGetBinder).GetHashCode();

        public VBGetBinder(string MemberName, CallInfo CallInfo) : base(MemberName, true, CallInfo)
        {
        }

        public override bool Equals(object _other)
        {
            VBGetBinder binder = _other as VBGetBinder;
            return (((binder != null) && string.Equals(this.Name, binder.Name)) && this.CallInfo.Equals(binder.CallInfo));
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] packedArgs, DynamicMetaObject errorSuggestion)
        {
            return new VBInvokeBinder(this.CallInfo, false).FallbackInvoke(target, packedArgs, errorSuggestion);
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
            if ((errorSuggestion != null) && !NewLateBinding.CanBindGet(target.Value, this.Name, argValues, argNames))
            {
                return errorSuggestion;
            }
            ParameterExpression left = Expression.Variable(typeof(object), "result");
            ParameterExpression expression = Expression.Variable(typeof(object[]), "array");
            Expression right = Expression.Call(typeof(NewLateBinding).GetMethod("FallbackGet"), target.Expression, Expression.Constant(this.Name), Expression.Assign(expression, Expression.NewArrayInit(typeof(object), args)), Expression.Constant(argNames, typeof(string[])));
            return new DynamicMetaObject(Expression.Block(new ParameterExpression[] { left, expression }, new Expression[] { Expression.Assign(left, right), IDOUtils.GetWriteBack(args, expression), left }), IDOUtils.CreateRestrictions(target, packedArgs, null));
        }

        public override int GetHashCode()
        {
            return ((_hash ^ this.Name.GetHashCode()) ^ this.CallInfo.GetHashCode());
        }
    }
}

