namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class TypedDynamicExpressionN : DynamicExpressionN
    {
        private readonly System.Type _returnType;

        internal TypedDynamicExpressionN(System.Type returnType, System.Type delegateType, CallSiteBinder binder, IList<Expression> arguments) : base(delegateType, binder, arguments)
        {
            this._returnType = returnType;
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._returnType;
            }
        }
    }
}

