namespace System.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [DebuggerDisplay("{DebugView}"), DebuggerTypeProxy(typeof(BindingRestrictions.BindingRestrictionsProxy))]
    public abstract class BindingRestrictions
    {
        private const int CustomRestrictionHash = 0x40000000;
        public static readonly BindingRestrictions Empty = new CustomRestriction(Expression.Constant(true));
        private const int InstanceRestrictionHash = 0x20000000;
        private const int TypeRestrictionHash = 0x10000000;

        private BindingRestrictions()
        {
        }

        public static BindingRestrictions Combine(IList<DynamicMetaObject> contributingObjects)
        {
            BindingRestrictions empty = Empty;
            if (contributingObjects != null)
            {
                foreach (DynamicMetaObject obj2 in contributingObjects)
                {
                    if (obj2 != null)
                    {
                        empty = empty.Merge(obj2.Restrictions);
                    }
                }
            }
            return empty;
        }

        internal abstract Expression GetExpression();
        public static BindingRestrictions GetExpressionRestriction(Expression expression)
        {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.Requires(expression.Type == typeof(bool), "expression");
            return new CustomRestriction(expression);
        }

        public static BindingRestrictions GetInstanceRestriction(Expression expression, object instance)
        {
            ContractUtils.RequiresNotNull(expression, "expression");
            return new InstanceRestriction(expression, instance);
        }

        internal static BindingRestrictions GetTypeRestriction(DynamicMetaObject obj)
        {
            if ((obj.Value == null) && obj.HasValue)
            {
                return GetInstanceRestriction(obj.Expression, null);
            }
            return GetTypeRestriction(obj.Expression, obj.LimitType);
        }

        public static BindingRestrictions GetTypeRestriction(Expression expression, Type type)
        {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            return new TypeRestriction(expression, type);
        }

        public BindingRestrictions Merge(BindingRestrictions restrictions)
        {
            ContractUtils.RequiresNotNull(restrictions, "restrictions");
            if (this == Empty)
            {
                return restrictions;
            }
            if (restrictions == Empty)
            {
                return this;
            }
            return new MergedRestriction(this, restrictions);
        }

        public Expression ToExpression()
        {
            if (this == Empty)
            {
                return Expression.Constant(true);
            }
            TestBuilder builder = new TestBuilder();
            Stack<BindingRestrictions> stack = new Stack<BindingRestrictions>();
            stack.Push(this);
            do
            {
                BindingRestrictions restrictions = stack.Pop();
                MergedRestriction restriction = restrictions as MergedRestriction;
                if (restriction != null)
                {
                    stack.Push(restriction.Right);
                    stack.Push(restriction.Left);
                }
                else
                {
                    builder.Append(restrictions);
                }
            }
            while (stack.Count > 0);
            return builder.ToExpression();
        }

        private string DebugView
        {
            get
            {
                return this.ToExpression().ToString();
            }
        }

        private sealed class BindingRestrictionsProxy
        {
            private readonly BindingRestrictions _node;

            public BindingRestrictionsProxy(BindingRestrictions node)
            {
                this._node = node;
            }

            public override string ToString()
            {
                return this._node.DebugView;
            }

            public bool IsEmpty
            {
                get
                {
                    return (this._node == BindingRestrictions.Empty);
                }
            }

            public BindingRestrictions[] Restrictions
            {
                get
                {
                    List<BindingRestrictions> list = new List<BindingRestrictions>();
                    Stack<BindingRestrictions> stack = new Stack<BindingRestrictions>();
                    stack.Push(this._node);
                    do
                    {
                        BindingRestrictions item = stack.Pop();
                        BindingRestrictions.MergedRestriction restriction = item as BindingRestrictions.MergedRestriction;
                        if (restriction != null)
                        {
                            stack.Push(restriction.Right);
                            stack.Push(restriction.Left);
                        }
                        else
                        {
                            list.Add(item);
                        }
                    }
                    while (stack.Count > 0);
                    return list.ToArray();
                }
            }

            public Expression Test
            {
                get
                {
                    return this._node.ToExpression();
                }
            }
        }

        private sealed class CustomRestriction : BindingRestrictions
        {
            private readonly Expression _expression;

            internal CustomRestriction(Expression expression)
            {
                this._expression = expression;
            }

            public override bool Equals(object obj)
            {
                BindingRestrictions.CustomRestriction restriction = obj as BindingRestrictions.CustomRestriction;
                return ((restriction != null) && (restriction._expression == this._expression));
            }

            internal override Expression GetExpression()
            {
                return this._expression;
            }

            public override int GetHashCode()
            {
                return (0x40000000 ^ this._expression.GetHashCode());
            }
        }

        private sealed class InstanceRestriction : BindingRestrictions
        {
            private readonly Expression _expression;
            private readonly object _instance;

            internal InstanceRestriction(Expression parameter, object instance)
            {
                this._expression = parameter;
                this._instance = instance;
            }

            public override bool Equals(object obj)
            {
                BindingRestrictions.InstanceRestriction restriction = obj as BindingRestrictions.InstanceRestriction;
                return (((restriction != null) && (restriction._instance == this._instance)) && (restriction._expression == this._expression));
            }

            internal override Expression GetExpression()
            {
                if (this._instance == null)
                {
                    return Expression.Equal(Expression.Convert(this._expression, typeof(object)), Expression.Constant(null));
                }
                ParameterExpression left = Expression.Parameter(typeof(object), null);
                return Expression.Block(new ParameterExpression[] { left }, new Expression[] { Expression.Assign(left, Expression.Property(Expression.Constant(new WeakReference(this._instance)), typeof(WeakReference).GetProperty("Target"))), Expression.AndAlso(Expression.NotEqual(left, Expression.Constant(null)), Expression.Equal(Expression.Convert(this._expression, typeof(object)), left)) });
            }

            public override int GetHashCode()
            {
                return ((0x20000000 ^ RuntimeHelpers.GetHashCode(this._instance)) ^ this._expression.GetHashCode());
            }
        }

        private sealed class MergedRestriction : BindingRestrictions
        {
            internal readonly BindingRestrictions Left;
            internal readonly BindingRestrictions Right;

            internal MergedRestriction(BindingRestrictions left, BindingRestrictions right)
            {
                this.Left = left;
                this.Right = right;
            }

            internal override Expression GetExpression()
            {
                throw ContractUtils.Unreachable;
            }
        }

        private sealed class TestBuilder
        {
            private readonly Stack<AndNode> _tests = new Stack<AndNode>();
            private readonly Set<BindingRestrictions> _unique = new Set<BindingRestrictions>();

            internal void Append(BindingRestrictions restrictions)
            {
                if (!this._unique.Contains(restrictions))
                {
                    this._unique.Add(restrictions);
                    this.Push(restrictions.GetExpression(), 0);
                }
            }

            private void Push(Expression node, int depth)
            {
                while ((this._tests.Count > 0) && (this._tests.Peek().Depth == depth))
                {
                    node = Expression.AndAlso(this._tests.Pop().Node, node);
                    depth++;
                }
                AndNode item = new AndNode {
                    Node = node,
                    Depth = depth
                };
                this._tests.Push(item);
            }

            internal Expression ToExpression()
            {
                Expression node = this._tests.Pop().Node;
                while (this._tests.Count > 0)
                {
                    node = Expression.AndAlso(this._tests.Pop().Node, node);
                }
                return node;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct AndNode
            {
                internal int Depth;
                internal Expression Node;
            }
        }

        private sealed class TypeRestriction : BindingRestrictions
        {
            private readonly Expression _expression;
            private readonly Type _type;

            internal TypeRestriction(Expression parameter, Type type)
            {
                this._expression = parameter;
                this._type = type;
            }

            public override bool Equals(object obj)
            {
                BindingRestrictions.TypeRestriction restriction = obj as BindingRestrictions.TypeRestriction;
                return (((restriction != null) && TypeUtils.AreEquivalent(restriction._type, this._type)) && (restriction._expression == this._expression));
            }

            internal override Expression GetExpression()
            {
                return Expression.TypeEqual(this._expression, this._type);
            }

            public override int GetHashCode()
            {
                return ((0x10000000 ^ this._expression.GetHashCode()) ^ this._type.GetHashCode());
            }
        }
    }
}

