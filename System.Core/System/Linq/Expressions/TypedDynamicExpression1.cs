namespace System.Linq.Expressions
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class TypedDynamicExpression1 : DynamicExpression1
    {
        private readonly System.Type _retType;

        internal TypedDynamicExpression1(System.Type retType, System.Type delegateType, CallSiteBinder binder, Expression arg0) : base(delegateType, binder, arg0)
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

