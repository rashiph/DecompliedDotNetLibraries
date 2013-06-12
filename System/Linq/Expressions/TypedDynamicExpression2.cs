namespace System.Linq.Expressions
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class TypedDynamicExpression2 : DynamicExpression2
    {
        private readonly System.Type _retType;

        internal TypedDynamicExpression2(System.Type retType, System.Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1) : base(delegateType, binder, arg0, arg1)
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

