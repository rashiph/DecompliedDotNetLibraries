namespace System.Linq.Expressions
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class TypedDynamicExpression4 : DynamicExpression4
    {
        private readonly System.Type _retType;

        internal TypedDynamicExpression4(System.Type retType, System.Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3) : base(delegateType, binder, arg0, arg1, arg2, arg3)
        {
            this._retType = retType;
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._retType;
            }
        }
    }
}

