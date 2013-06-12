namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Runtime.CompilerServices;

    [DebuggerTypeProxy(typeof(Expression.MemberInitExpressionProxy))]
    public sealed class MemberInitExpression : Expression
    {
        private readonly ReadOnlyCollection<MemberBinding> _bindings;
        private readonly System.Linq.Expressions.NewExpression _newExpression;

        internal MemberInitExpression(System.Linq.Expressions.NewExpression newExpression, ReadOnlyCollection<MemberBinding> bindings)
        {
            this._newExpression = newExpression;
            this._bindings = bindings;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitMemberInit(this);
        }

        public override Expression Reduce()
        {
            return ReduceMemberInit(this._newExpression, this._bindings, true);
        }

        internal static Expression ReduceListInit(Expression listExpression, ReadOnlyCollection<ElementInit> initializers, bool keepOnStack)
        {
            ParameterExpression left = Expression.Variable(listExpression.Type, null);
            int count = initializers.Count;
            Expression[] list = new Expression[count + 2];
            list[0] = Expression.Assign(left, listExpression);
            for (int i = 0; i < count; i++)
            {
                ElementInit init = initializers[i];
                list[i + 1] = Expression.Call(left, init.AddMethod, init.Arguments);
            }
            list[count + 1] = keepOnStack ? ((Expression) left) : ((Expression) Expression.Empty());
            return Expression.Block(new TrueReadOnlyCollection<Expression>(list));
        }

        internal static Expression ReduceMemberBinding(ParameterExpression objVar, MemberBinding binding)
        {
            MemberExpression left = Expression.MakeMemberAccess(objVar, binding.Member);
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return Expression.Assign(left, ((MemberAssignment) binding).Expression);

                case MemberBindingType.MemberBinding:
                    return ReduceMemberInit(left, ((MemberMemberBinding) binding).Bindings, false);

                case MemberBindingType.ListBinding:
                    return ReduceListInit(left, ((MemberListBinding) binding).Initializers, false);
            }
            throw ContractUtils.Unreachable;
        }

        internal static Expression ReduceMemberInit(Expression objExpression, ReadOnlyCollection<MemberBinding> bindings, bool keepOnStack)
        {
            ParameterExpression left = Expression.Variable(objExpression.Type, null);
            int count = bindings.Count;
            Expression[] list = new Expression[count + 2];
            list[0] = Expression.Assign(left, objExpression);
            for (int i = 0; i < count; i++)
            {
                list[i + 1] = ReduceMemberBinding(left, bindings[i]);
            }
            list[count + 1] = keepOnStack ? ((Expression) left) : ((Expression) Expression.Empty());
            return Expression.Block(new TrueReadOnlyCollection<Expression>(list));
        }

        public MemberInitExpression Update(System.Linq.Expressions.NewExpression newExpression, IEnumerable<MemberBinding> bindings)
        {
            if ((newExpression == this.NewExpression) && (bindings == this.Bindings))
            {
                return this;
            }
            return Expression.MemberInit(newExpression, bindings);
        }

        public ReadOnlyCollection<MemberBinding> Bindings
        {
            get
            {
                return this._bindings;
            }
        }

        public override bool CanReduce
        {
            get
            {
                return true;
            }
        }

        public System.Linq.Expressions.NewExpression NewExpression
        {
            get
            {
                return this._newExpression;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.MemberInit;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._newExpression.Type;
            }
        }
    }
}

