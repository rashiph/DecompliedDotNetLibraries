namespace System.Runtime.CompilerServices
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Linq.Expressions.Compiler;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [EditorBrowsable(EditorBrowsableState.Never), DebuggerStepThrough]
    public static class RuntimeOps
    {
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("do not use this method", true)]
        public static IRuntimeVariables CreateRuntimeVariables()
        {
            return new EmptyRuntimeVariables();
        }

        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static IRuntimeVariables CreateRuntimeVariables(object[] data, long[] indexes)
        {
            return new RuntimeVariableList(data, indexes);
        }

        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool ExpandoCheckVersion(ExpandoObject expando, object version)
        {
            return (expando.Class == version);
        }

        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static void ExpandoPromoteClass(ExpandoObject expando, object oldClass, object newClass)
        {
            expando.PromoteClass(oldClass, newClass);
        }

        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool ExpandoTryDeleteValue(ExpandoObject expando, object indexClass, int index, string name, bool ignoreCase)
        {
            return expando.TryDeleteValue(indexClass, index, name, ignoreCase, ExpandoObject.Uninitialized);
        }

        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool ExpandoTryGetValue(ExpandoObject expando, object indexClass, int index, string name, bool ignoreCase, out object value)
        {
            return expando.TryGetValue(indexClass, index, name, ignoreCase, out value);
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("do not use this method", true)]
        public static object ExpandoTrySetValue(ExpandoObject expando, object indexClass, int index, object value, string name, bool ignoreCase)
        {
            expando.TrySetValue(indexClass, index, value, name, ignoreCase, false);
            return value;
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("do not use this method", true)]
        public static IRuntimeVariables MergeRuntimeVariables(IRuntimeVariables first, IRuntimeVariables second, int[] indexes)
        {
            return new MergedRuntimeVariables(first, second, indexes);
        }

        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static Expression Quote(Expression expression, object hoistedLocals, object[] locals)
        {
            ExpressionQuoter quoter = new ExpressionQuoter((HoistedLocals) hoistedLocals, locals);
            return quoter.Visit(expression);
        }

        private sealed class EmptyRuntimeVariables : IRuntimeVariables
        {
            int IRuntimeVariables.Count
            {
                get
                {
                    return 0;
                }
            }

            object IRuntimeVariables.this[int index]
            {
                get
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                set
                {
                    throw new ArgumentOutOfRangeException("index");
                }
            }
        }

        private sealed class ExpressionQuoter : ExpressionVisitor
        {
            private readonly object[] _locals;
            private readonly HoistedLocals _scope;
            private readonly Stack<Set<ParameterExpression>> _shadowedVars = new Stack<Set<ParameterExpression>>();

            internal ExpressionQuoter(HoistedLocals scope, object[] locals)
            {
                this._scope = scope;
                this._locals = locals;
            }

            private IStrongBox GetBox(ParameterExpression variable)
            {
                int num;
                foreach (Set<ParameterExpression> set in this._shadowedVars)
                {
                    if (set.Contains(variable))
                    {
                        return null;
                    }
                }
                HoistedLocals parent = this._scope;
                object[] objArray = this._locals;
                while (!parent.Indexes.TryGetValue(variable, out num))
                {
                    parent = parent.Parent;
                    if (parent == null)
                    {
                        throw ContractUtils.Unreachable;
                    }
                    objArray = HoistedLocals.GetParent(objArray);
                }
                return (IStrongBox) objArray[num];
            }

            protected internal override Expression VisitBlock(BlockExpression node)
            {
                if (node.Variables.Count > 0)
                {
                    this._shadowedVars.Push(new Set<ParameterExpression>(node.Variables));
                }
                ReadOnlyCollection<Expression> onlys = base.Visit(node.Expressions);
                if (node.Variables.Count > 0)
                {
                    this._shadowedVars.Pop();
                }
                if (onlys == node.Expressions)
                {
                    return node;
                }
                return Expression.Block((IEnumerable<ParameterExpression>) node.Variables, (IEnumerable<Expression>) onlys);
            }

            protected override CatchBlock VisitCatchBlock(CatchBlock node)
            {
                if (node.Variable != null)
                {
                    this._shadowedVars.Push(new Set<ParameterExpression>(new ParameterExpression[] { node.Variable }));
                }
                Expression body = this.Visit(node.Body);
                Expression filter = this.Visit(node.Filter);
                if (node.Variable != null)
                {
                    this._shadowedVars.Pop();
                }
                if ((body == node.Body) && (filter == node.Filter))
                {
                    return node;
                }
                return Expression.MakeCatchBlock(node.Test, node.Variable, body, filter);
            }

            protected internal override Expression VisitLambda<T>(Expression<T> node)
            {
                this._shadowedVars.Push(new Set<ParameterExpression>(node.Parameters));
                Expression body = this.Visit(node.Body);
                this._shadowedVars.Pop();
                if (body == node.Body)
                {
                    return node;
                }
                return Expression.Lambda<T>(body, node.Name, node.TailCall, node.Parameters);
            }

            protected internal override Expression VisitParameter(ParameterExpression node)
            {
                IStrongBox box = this.GetBox(node);
                if (box == null)
                {
                    return node;
                }
                return Expression.Field(Expression.Constant(box), "Value");
            }

            protected internal override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
            {
                int count = node.Variables.Count;
                List<IStrongBox> list = new List<IStrongBox>();
                List<ParameterExpression> list2 = new List<ParameterExpression>();
                int[] numArray = new int[count];
                for (int i = 0; i < count; i++)
                {
                    IStrongBox item = this.GetBox(node.Variables[i]);
                    if (item == null)
                    {
                        numArray[i] = list2.Count;
                        list2.Add(node.Variables[i]);
                    }
                    else
                    {
                        numArray[i] = -1 - list.Count;
                        list.Add(item);
                    }
                }
                if (list.Count == 0)
                {
                    return node;
                }
                ConstantExpression expression = Expression.Constant(new RuntimeOps.RuntimeVariables(list.ToArray()), typeof(IRuntimeVariables));
                if (list2.Count == 0)
                {
                    return expression;
                }
                return Expression.Call(typeof(RuntimeOps).GetMethod("MergeRuntimeVariables"), Expression.RuntimeVariables(new TrueReadOnlyCollection<ParameterExpression>(list2.ToArray())), expression, Expression.Constant(numArray));
            }
        }

        private sealed class MergedRuntimeVariables : IRuntimeVariables
        {
            private readonly IRuntimeVariables _first;
            private readonly int[] _indexes;
            private readonly IRuntimeVariables _second;

            internal MergedRuntimeVariables(IRuntimeVariables first, IRuntimeVariables second, int[] indexes)
            {
                this._first = first;
                this._second = second;
                this._indexes = indexes;
            }

            public int Count
            {
                get
                {
                    return this._indexes.Length;
                }
            }

            public object this[int index]
            {
                get
                {
                    index = this._indexes[index];
                    if (index < 0)
                    {
                        return this._second[-1 - index];
                    }
                    return this._first[index];
                }
                set
                {
                    index = this._indexes[index];
                    if (index >= 0)
                    {
                        this._first[index] = value;
                    }
                    else
                    {
                        this._second[-1 - index] = value;
                    }
                }
            }
        }

        private sealed class RuntimeVariableList : IRuntimeVariables
        {
            private readonly object[] _data;
            private readonly long[] _indexes;

            internal RuntimeVariableList(object[] data, long[] indexes)
            {
                this._data = data;
                this._indexes = indexes;
            }

            private IStrongBox GetStrongBox(int index)
            {
                long num = this._indexes[index];
                object[] locals = this._data;
                for (int i = (int) (num >> 0x20); i > 0; i--)
                {
                    locals = HoistedLocals.GetParent(locals);
                }
                return (IStrongBox) locals[(int) num];
            }

            public int Count
            {
                get
                {
                    return this._indexes.Length;
                }
            }

            public object this[int index]
            {
                get
                {
                    return this.GetStrongBox(index).Value;
                }
                set
                {
                    this.GetStrongBox(index).Value = value;
                }
            }
        }

        private sealed class RuntimeVariables : IRuntimeVariables
        {
            private readonly IStrongBox[] _boxes;

            internal RuntimeVariables(IStrongBox[] boxes)
            {
                this._boxes = boxes;
            }

            int IRuntimeVariables.Count
            {
                get
                {
                    return this._boxes.Length;
                }
            }

            object IRuntimeVariables.this[int index]
            {
                get
                {
                    return this._boxes[index].Value;
                }
                set
                {
                    this._boxes[index].Value = value;
                }
            }
        }
    }
}

