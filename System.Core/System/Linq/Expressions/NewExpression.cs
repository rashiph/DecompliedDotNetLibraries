namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;

    [DebuggerTypeProxy(typeof(Expression.NewExpressionProxy))]
    public class NewExpression : Expression, IArgumentProvider
    {
        private IList<Expression> _arguments;
        private readonly ConstructorInfo _constructor;
        private readonly ReadOnlyCollection<MemberInfo> _members;

        internal NewExpression(ConstructorInfo constructor, IList<Expression> arguments, ReadOnlyCollection<MemberInfo> members)
        {
            this._constructor = constructor;
            this._arguments = arguments;
            this._members = members;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitNew(this);
        }

        Expression IArgumentProvider.GetArgument(int index)
        {
            return this._arguments[index];
        }

        public NewExpression Update(IEnumerable<Expression> arguments)
        {
            if (arguments == this.Arguments)
            {
                return this;
            }
            if (this.Members != null)
            {
                return Expression.New(this.Constructor, arguments, this.Members);
            }
            return Expression.New(this.Constructor, arguments);
        }

        public ReadOnlyCollection<Expression> Arguments
        {
            get
            {
                return Expression.ReturnReadOnly<Expression>(ref this._arguments);
            }
        }

        public ConstructorInfo Constructor
        {
            get
            {
                return this._constructor;
            }
        }

        public ReadOnlyCollection<MemberInfo> Members
        {
            get
            {
                return this._members;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.New;
            }
        }

        int IArgumentProvider.ArgumentCount
        {
            get
            {
                return this._arguments.Count;
            }
        }

        public override System.Type Type
        {
            get
            {
                return this._constructor.DeclaringType;
            }
        }
    }
}

