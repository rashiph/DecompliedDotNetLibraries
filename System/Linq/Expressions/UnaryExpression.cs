namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [DebuggerTypeProxy(typeof(Expression.UnaryExpressionProxy))]
    public sealed class UnaryExpression : Expression
    {
        private readonly MethodInfo _method;
        private readonly ExpressionType _nodeType;
        private readonly Expression _operand;
        private readonly System.Type _type;

        internal UnaryExpression(ExpressionType nodeType, Expression expression, System.Type type, MethodInfo method)
        {
            this._operand = expression;
            this._method = method;
            this._nodeType = nodeType;
            this._type = type;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitUnary(this);
        }

        private UnaryExpression FunctionalOp(Expression operand)
        {
            ExpressionType increment;
            if ((this._nodeType == ExpressionType.PreIncrementAssign) || (this._nodeType == ExpressionType.PostIncrementAssign))
            {
                increment = ExpressionType.Increment;
            }
            else
            {
                increment = ExpressionType.Decrement;
            }
            return new UnaryExpression(increment, operand, operand.Type, this._method);
        }

        public override Expression Reduce()
        {
            if (!this.CanReduce)
            {
                return this;
            }
            ExpressionType nodeType = this._operand.NodeType;
            if (nodeType != ExpressionType.MemberAccess)
            {
                if (nodeType == ExpressionType.Index)
                {
                    return this.ReduceIndex();
                }
                return this.ReduceVariable();
            }
            return this.ReduceMember();
        }

        private Expression ReduceIndex()
        {
            bool isPrefix = this.IsPrefix;
            IndexExpression right = (IndexExpression) this._operand;
            int count = right.Arguments.Count;
            Expression[] list = new Expression[count + (isPrefix ? 2 : 4)];
            ParameterExpression[] expressionArray2 = new ParameterExpression[count + (isPrefix ? 1 : 2)];
            ParameterExpression[] expressionArray3 = new ParameterExpression[count];
            int index = 0;
            expressionArray2[index] = Expression.Parameter(right.Object.Type, null);
            list[index] = Expression.Assign(expressionArray2[index], right.Object);
            index++;
            while (index <= count)
            {
                Expression expression2 = right.Arguments[index - 1];
                expressionArray3[index - 1] = expressionArray2[index] = Expression.Parameter(expression2.Type, null);
                list[index] = Expression.Assign(expressionArray2[index], expression2);
                index++;
            }
            right = Expression.MakeIndex(expressionArray2[0], right.Indexer, new TrueReadOnlyCollection<Expression>(expressionArray3));
            if (!isPrefix)
            {
                ParameterExpression operand = expressionArray2[index] = Expression.Parameter(right.Type, null);
                list[index] = Expression.Assign(expressionArray2[index], right);
                index++;
                list[index++] = Expression.Assign(right, this.FunctionalOp(operand));
                list[index++] = operand;
            }
            else
            {
                list[index++] = Expression.Assign(right, this.FunctionalOp(right));
            }
            return Expression.Block((IEnumerable<ParameterExpression>) new TrueReadOnlyCollection<ParameterExpression>(expressionArray2), (IEnumerable<Expression>) new TrueReadOnlyCollection<Expression>(list));
        }

        private Expression ReduceMember()
        {
            MemberExpression left = (MemberExpression) this._operand;
            if (left.Expression == null)
            {
                return this.ReduceVariable();
            }
            ParameterExpression expression2 = Expression.Parameter(left.Expression.Type, null);
            BinaryExpression expression3 = Expression.Assign(expression2, left.Expression);
            left = Expression.MakeMemberAccess(expression2, left.Member);
            if (this.IsPrefix)
            {
                return Expression.Block(new ParameterExpression[] { expression2 }, new Expression[] { expression3, Expression.Assign(left, this.FunctionalOp(left)) });
            }
            ParameterExpression expression4 = Expression.Parameter(left.Type, null);
            return Expression.Block(new ParameterExpression[] { expression2, expression4 }, new Expression[] { expression3, Expression.Assign(expression4, left), Expression.Assign(left, this.FunctionalOp(expression4)), expression4 });
        }

        private Expression ReduceVariable()
        {
            if (this.IsPrefix)
            {
                return Expression.Assign(this._operand, this.FunctionalOp(this._operand));
            }
            ParameterExpression left = Expression.Parameter(this._operand.Type, null);
            return Expression.Block(new ParameterExpression[] { left }, new Expression[] { Expression.Assign(left, this._operand), Expression.Assign(this._operand, this.FunctionalOp(left)), left });
        }

        public UnaryExpression Update(Expression operand)
        {
            if (operand == this.Operand)
            {
                return this;
            }
            return Expression.MakeUnary(this.NodeType, operand, this.Type, this.Method);
        }

        public override bool CanReduce
        {
            get
            {
                switch (this._nodeType)
                {
                    case ExpressionType.PreIncrementAssign:
                    case ExpressionType.PreDecrementAssign:
                    case ExpressionType.PostIncrementAssign:
                    case ExpressionType.PostDecrementAssign:
                        return true;
                }
                return false;
            }
        }

        public bool IsLifted
        {
            get
            {
                if (((this.NodeType == ExpressionType.TypeAs) || (this.NodeType == ExpressionType.Quote)) || (this.NodeType == ExpressionType.Throw))
                {
                    return false;
                }
                bool flag = this._operand.Type.IsNullableType();
                bool flag2 = this.Type.IsNullableType();
                if (this._method != null)
                {
                    return ((flag && !TypeUtils.AreEquivalent(this._method.GetParametersCached()[0].ParameterType, this._operand.Type)) || (flag2 && !TypeUtils.AreEquivalent(this._method.ReturnType, this.Type)));
                }
                if (!flag)
                {
                    return flag2;
                }
                return true;
            }
        }

        public bool IsLiftedToNull
        {
            get
            {
                return (this.IsLifted && this.Type.IsNullableType());
            }
        }

        private bool IsPrefix
        {
            get
            {
                if (this._nodeType != ExpressionType.PreIncrementAssign)
                {
                    return (this._nodeType == ExpressionType.PreDecrementAssign);
                }
                return true;
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
                return this._nodeType;
            }
        }

        public Expression Operand
        {
            get
            {
                return this._operand;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

