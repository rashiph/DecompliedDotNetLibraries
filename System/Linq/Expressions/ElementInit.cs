namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    public sealed class ElementInit : IArgumentProvider
    {
        private MethodInfo _addMethod;
        private ReadOnlyCollection<Expression> _arguments;

        internal ElementInit(MethodInfo addMethod, ReadOnlyCollection<Expression> arguments)
        {
            this._addMethod = addMethod;
            this._arguments = arguments;
        }

        Expression IArgumentProvider.GetArgument(int index)
        {
            return this._arguments[index];
        }

        public override string ToString()
        {
            return ExpressionStringBuilder.ElementInitBindingToString(this);
        }

        public ElementInit Update(IEnumerable<Expression> arguments)
        {
            if (arguments == this.Arguments)
            {
                return this;
            }
            return Expression.ElementInit(this.AddMethod, arguments);
        }

        public MethodInfo AddMethod
        {
            get
            {
                return this._addMethod;
            }
        }

        public ReadOnlyCollection<Expression> Arguments
        {
            get
            {
                return this._arguments;
            }
        }

        int IArgumentProvider.ArgumentCount
        {
            get
            {
                return this._arguments.Count;
            }
        }
    }
}

