namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    internal class MethodCallExpression1 : MethodCallExpression, IArgumentProvider
    {
        private object _arg0;

        public MethodCallExpression1(MethodInfo method, Expression arg0) : base(method)
        {
            this._arg0 = arg0;
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments()
        {
            return Expression.ReturnReadOnly(this, ref this._arg0);
        }

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args)
        {
            if (args != null)
            {
                return Expression.Call(base.Method, args[0]);
            }
            return Expression.Call(base.Method, Expression.ReturnObject<Expression>(this._arg0));
        }

        Expression IArgumentProvider.GetArgument(int index)
        {
            if (index != 0)
            {
                throw new InvalidOperationException();
            }
            return Expression.ReturnObject<Expression>(this._arg0);
        }

        int IArgumentProvider.ArgumentCount
        {
            get
            {
                return 1;
            }
        }
    }
}

