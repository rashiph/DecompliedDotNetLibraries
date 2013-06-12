namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Reflection;

    [DebuggerTypeProxy(typeof(Expression.BinaryExpressionProxy))]
    public class BinaryExpression : Expression
    {
        private readonly Expression _left;
        private readonly Expression _right;

        internal BinaryExpression(Expression left, Expression right)
        {
            this._left = left;
            this._right = right;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitBinary(this);
        }

        internal static Expression Create(ExpressionType nodeType, Expression left, Expression right, Type type, MethodInfo method, LambdaExpression conversion)
        {
            if (nodeType == ExpressionType.Assign)
            {
                return new AssignBinaryExpression(left, right);
            }
            if (conversion != null)
            {
                return new CoalesceConversionBinaryExpression(left, right, conversion);
            }
            if (method != null)
            {
                return new MethodBinaryExpression(nodeType, left, right, type, method);
            }
            if (type == typeof(bool))
            {
                return new LogicalBinaryExpression(nodeType, left, right);
            }
            return new SimpleBinaryExpression(nodeType, left, right, type);
        }

        private static ExpressionType GetBinaryOpFromAssignmentOp(ExpressionType op)
        {
            switch (op)
            {
                case ExpressionType.AddAssign:
                    return ExpressionType.Add;

                case ExpressionType.AndAssign:
                    return ExpressionType.And;

                case ExpressionType.DivideAssign:
                    return ExpressionType.Divide;

                case ExpressionType.ExclusiveOrAssign:
                    return ExpressionType.ExclusiveOr;

                case ExpressionType.LeftShiftAssign:
                    return ExpressionType.LeftShift;

                case ExpressionType.ModuloAssign:
                    return ExpressionType.Modulo;

                case ExpressionType.MultiplyAssign:
                    return ExpressionType.Multiply;

                case ExpressionType.OrAssign:
                    return ExpressionType.Or;

                case ExpressionType.PowerAssign:
                    return ExpressionType.Power;

                case ExpressionType.RightShiftAssign:
                    return ExpressionType.RightShift;

                case ExpressionType.SubtractAssign:
                    return ExpressionType.Subtract;

                case ExpressionType.AddAssignChecked:
                    return ExpressionType.AddChecked;

                case ExpressionType.MultiplyAssignChecked:
                    return ExpressionType.MultiplyChecked;

                case ExpressionType.SubtractAssignChecked:
                    return ExpressionType.SubtractChecked;
            }
            throw Error.InvalidOperation("op");
        }

        internal virtual LambdaExpression GetConversion()
        {
            return null;
        }

        internal virtual MethodInfo GetMethod()
        {
            return null;
        }

        private static bool IsOpAssignment(ExpressionType op)
        {
            switch (op)
            {
                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                    return true;
            }
            return false;
        }

        public override Expression Reduce()
        {
            if (!IsOpAssignment(this.NodeType))
            {
                return this;
            }
            ExpressionType nodeType = this._left.NodeType;
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
            IndexExpression expression = (IndexExpression) this._left;
            List<ParameterExpression> list = new List<ParameterExpression>(expression.Arguments.Count + 2);
            List<Expression> list2 = new List<Expression>(expression.Arguments.Count + 3);
            ParameterExpression item = Expression.Variable(expression.Object.Type, "tempObj");
            list.Add(item);
            list2.Add(Expression.Assign(item, expression.Object));
            List<Expression> arguments = new List<Expression>(expression.Arguments.Count);
            foreach (Expression expression3 in expression.Arguments)
            {
                ParameterExpression expression4 = Expression.Variable(expression3.Type, "tempArg" + arguments.Count);
                list.Add(expression4);
                arguments.Add(expression4);
                list2.Add(Expression.Assign(expression4, expression3));
            }
            IndexExpression left = Expression.MakeIndex(item, expression.Indexer, arguments);
            Expression right = Expression.MakeBinary(GetBinaryOpFromAssignmentOp(this.NodeType), left, this._right, false, this.Method);
            LambdaExpression conversion = this.GetConversion();
            if (conversion != null)
            {
                right = Expression.Invoke(conversion, new Expression[] { right });
            }
            ParameterExpression expression8 = Expression.Variable(right.Type, "tempValue");
            list.Add(expression8);
            list2.Add(Expression.Assign(expression8, right));
            list2.Add(Expression.Assign(left, expression8));
            return Expression.Block((IEnumerable<ParameterExpression>) list, (IEnumerable<Expression>) list2);
        }

        private Expression ReduceMember()
        {
            MemberExpression expression = (MemberExpression) this._left;
            if (expression.Expression == null)
            {
                return this.ReduceVariable();
            }
            ParameterExpression left = Expression.Variable(expression.Expression.Type, "temp1");
            Expression expression3 = Expression.Assign(left, expression.Expression);
            Expression right = Expression.MakeBinary(GetBinaryOpFromAssignmentOp(this.NodeType), Expression.MakeMemberAccess(left, expression.Member), this._right, false, this.Method);
            LambdaExpression conversion = this.GetConversion();
            if (conversion != null)
            {
                right = Expression.Invoke(conversion, new Expression[] { right });
            }
            ParameterExpression expression6 = Expression.Variable(right.Type, "temp2");
            right = Expression.Assign(expression6, right);
            Expression expression7 = Expression.Assign(Expression.MakeMemberAccess(left, expression.Member), expression6);
            Expression expression8 = expression6;
            return Expression.Block(new ParameterExpression[] { left, expression6 }, new Expression[] { expression3, right, expression7, expression8 });
        }

        internal Expression ReduceUserdefinedLifted()
        {
            ParameterExpression left = Expression.Parameter(this._left.Type, "left");
            ParameterExpression expression2 = Expression.Parameter(this.Right.Type, "right");
            string name = (this.NodeType == ExpressionType.AndAlso) ? "op_False" : "op_True";
            MethodInfo booleanOperator = TypeUtils.GetBooleanOperator(this.Method.DeclaringType, name);
            return Expression.Block(new ParameterExpression[] { left }, new Expression[] { Expression.Assign(left, this._left), Expression.Condition(Expression.Property(left, "HasValue"), Expression.Condition(Expression.Call(booleanOperator, Expression.Call((Expression) left, "GetValueOrDefault", (Type[]) null, new Expression[0])), left, Expression.Block(new ParameterExpression[] { expression2 }, new Expression[] { Expression.Assign(expression2, this._right), Expression.Condition(Expression.Property(expression2, "HasValue"), Expression.Convert(Expression.Call(this.Method, Expression.Call((Expression) left, "GetValueOrDefault", (Type[]) null, new Expression[0]), Expression.Call((Expression) expression2, "GetValueOrDefault", (Type[]) null, new Expression[0])), this.Type), Expression.Constant(null, this.Type)) })), Expression.Constant(null, this.Type)) });
        }

        private Expression ReduceVariable()
        {
            Expression right = Expression.MakeBinary(GetBinaryOpFromAssignmentOp(this.NodeType), this._left, this._right, false, this.Method);
            LambdaExpression conversion = this.GetConversion();
            if (conversion != null)
            {
                right = Expression.Invoke(conversion, new Expression[] { right });
            }
            return Expression.Assign(this._left, right);
        }

        public BinaryExpression Update(Expression left, LambdaExpression conversion, Expression right)
        {
            if (((left == this.Left) && (right == this.Right)) && (conversion == this.Conversion))
            {
                return this;
            }
            if (!this.IsReferenceComparison)
            {
                return Expression.MakeBinary(this.NodeType, left, right, this.IsLiftedToNull, this.Method, conversion);
            }
            if (this.NodeType == ExpressionType.Equal)
            {
                return Expression.ReferenceEqual(left, right);
            }
            return Expression.ReferenceNotEqual(left, right);
        }

        public override bool CanReduce
        {
            get
            {
                return IsOpAssignment(this.NodeType);
            }
        }

        public LambdaExpression Conversion
        {
            get
            {
                return this.GetConversion();
            }
        }

        public bool IsLifted
        {
            get
            {
                if ((this.NodeType == ExpressionType.Coalesce) || (this.NodeType == ExpressionType.Assign))
                {
                    return false;
                }
                if (!this._left.Type.IsNullableType())
                {
                    return false;
                }
                MethodInfo method = this.GetMethod();
                if (method != null)
                {
                    return !TypeUtils.AreEquivalent(method.GetParametersCached()[0].ParameterType.GetNonRefType(), this._left.Type);
                }
                return true;
            }
        }

        internal bool IsLiftedLogical
        {
            get
            {
                Type type = this._left.Type;
                Type type2 = this._right.Type;
                MethodInfo method = this.GetMethod();
                ExpressionType nodeType = this.NodeType;
                return ((((nodeType == ExpressionType.AndAlso) || (nodeType == ExpressionType.OrElse)) && ((TypeUtils.AreEquivalent(type2, type) && type.IsNullableType()) && (method != null))) && TypeUtils.AreEquivalent(method.ReturnType, type.GetNonNullableType()));
            }
        }

        public bool IsLiftedToNull
        {
            get
            {
                return (this.IsLifted && this.Type.IsNullableType());
            }
        }

        internal bool IsReferenceComparison
        {
            get
            {
                Type type = this._left.Type;
                Type type2 = this._right.Type;
                MethodInfo method = this.GetMethod();
                ExpressionType nodeType = this.NodeType;
                return ((((nodeType == ExpressionType.Equal) || (nodeType == ExpressionType.NotEqual)) && ((method == null) && !type.IsValueType)) && !type2.IsValueType);
            }
        }

        public Expression Left
        {
            get
            {
                return this._left;
            }
        }

        public MethodInfo Method
        {
            get
            {
                return this.GetMethod();
            }
        }

        public Expression Right
        {
            get
            {
                return this._right;
            }
        }
    }
}

