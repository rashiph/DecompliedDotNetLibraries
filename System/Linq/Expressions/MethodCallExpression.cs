namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Reflection;

    [DebuggerTypeProxy(typeof(Expression.MethodCallExpressionProxy))]
    public class MethodCallExpression : Expression, IArgumentProvider
    {
        private readonly MethodInfo _method;

        internal MethodCallExpression(MethodInfo method)
        {
            this._method = method;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitMethodCall(this);
        }

        internal virtual Expression GetInstance()
        {
            return null;
        }

        internal virtual ReadOnlyCollection<Expression> GetOrMakeArguments()
        {
            throw ContractUtils.Unreachable;
        }

        internal virtual MethodCallExpression Rewrite(Expression instance, IList<Expression> args)
        {
            throw ContractUtils.Unreachable;
        }

        Expression IArgumentProvider.GetArgument(int index)
        {
            throw ContractUtils.Unreachable;
        }

        public MethodCallExpression Update(Expression @object, IEnumerable<Expression> arguments)
        {
            if ((@object == this.Object) && (arguments == this.Arguments))
            {
                return this;
            }
            return Expression.Call(@object, this.Method, arguments);
        }

        public ReadOnlyCollection<Expression> Arguments
        {
            get
            {
                return this.GetOrMakeArguments();
            }
        }

        public MethodInfo Method
        {
            get
            {
                return this._method;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Call;
            }
        }

        public Expression Object
        {
            get
            {
                return this.GetInstance();
            }
        }

        int IArgumentProvider.ArgumentCount
        {
            get
            {
                throw ContractUtils.Unreachable;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._method.ReturnType;
            }
        }
    }
}

