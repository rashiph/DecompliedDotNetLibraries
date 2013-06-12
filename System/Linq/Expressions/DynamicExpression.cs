namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Runtime.CompilerServices;

    [DebuggerTypeProxy(typeof(Expression.DynamicExpressionProxy))]
    public class DynamicExpression : Expression, IArgumentProvider
    {
        private readonly CallSiteBinder _binder;
        private readonly System.Type _delegateType;

        internal DynamicExpression(System.Type delegateType, CallSiteBinder binder)
        {
            this._delegateType = delegateType;
            this._binder = binder;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitDynamic(this);
        }

        internal virtual ReadOnlyCollection<Expression> GetOrMakeArguments()
        {
            throw ContractUtils.Unreachable;
        }

        internal static DynamicExpression Make(System.Type returnType, System.Type delegateType, CallSiteBinder binder, ReadOnlyCollection<Expression> arguments)
        {
            if (returnType == typeof(object))
            {
                return new DynamicExpressionN(delegateType, binder, arguments);
            }
            return new TypedDynamicExpressionN(returnType, delegateType, binder, arguments);
        }

        internal static DynamicExpression Make(System.Type returnType, System.Type delegateType, CallSiteBinder binder, Expression arg0)
        {
            if (returnType == typeof(object))
            {
                return new DynamicExpression1(delegateType, binder, arg0);
            }
            return new TypedDynamicExpression1(returnType, delegateType, binder, arg0);
        }

        internal static DynamicExpression Make(System.Type returnType, System.Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1)
        {
            if (returnType == typeof(object))
            {
                return new DynamicExpression2(delegateType, binder, arg0, arg1);
            }
            return new TypedDynamicExpression2(returnType, delegateType, binder, arg0, arg1);
        }

        internal static DynamicExpression Make(System.Type returnType, System.Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2)
        {
            if (returnType == typeof(object))
            {
                return new DynamicExpression3(delegateType, binder, arg0, arg1, arg2);
            }
            return new TypedDynamicExpression3(returnType, delegateType, binder, arg0, arg1, arg2);
        }

        internal static DynamicExpression Make(System.Type returnType, System.Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
        {
            if (returnType == typeof(object))
            {
                return new DynamicExpression4(delegateType, binder, arg0, arg1, arg2, arg3);
            }
            return new TypedDynamicExpression4(returnType, delegateType, binder, arg0, arg1, arg2, arg3);
        }

        internal virtual DynamicExpression Rewrite(Expression[] args)
        {
            throw ContractUtils.Unreachable;
        }

        Expression IArgumentProvider.GetArgument(int index)
        {
            throw ContractUtils.Unreachable;
        }

        public DynamicExpression Update(IEnumerable<Expression> arguments)
        {
            if (arguments == this.Arguments)
            {
                return this;
            }
            return Expression.MakeDynamic(this.DelegateType, this.Binder, arguments);
        }

        public ReadOnlyCollection<Expression> Arguments
        {
            get
            {
                return this.GetOrMakeArguments();
            }
        }

        public CallSiteBinder Binder
        {
            get
            {
                return this._binder;
            }
        }

        public System.Type DelegateType
        {
            get
            {
                return this._delegateType;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Dynamic;
            }
        }

        int IArgumentProvider.ArgumentCount
        {
            get
            {
                throw ContractUtils.Unreachable;
            }
        }

        public override System.Type Type
        {
            get
            {
                return typeof(object);
            }
        }
    }
}

