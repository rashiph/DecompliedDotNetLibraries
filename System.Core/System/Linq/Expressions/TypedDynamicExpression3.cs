namespace System.Linq.Expressions
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class TypedDynamicExpression3 : DynamicExpression3
    {
        private readonly System.Type _retType;

        internal TypedDynamicExpression3(System.Type retType, System.Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2) : base(delegateType, binder, arg0, arg1, arg2)
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

