namespace System.Linq.Expressions.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal sealed class HoistedLocals
    {
        internal readonly ReadOnlyDictionary<Expression, int> Indexes;
        internal readonly HoistedLocals Parent;
        internal readonly ParameterExpression SelfVariable;
        internal readonly ReadOnlyCollection<ParameterExpression> Variables;

        internal HoistedLocals(HoistedLocals parent, ReadOnlyCollection<ParameterExpression> vars)
        {
            if (parent != null)
            {
                vars = new TrueReadOnlyCollection<ParameterExpression>(vars.AddFirst<ParameterExpression>(parent.SelfVariable));
            }
            Dictionary<Expression, int> dict = new Dictionary<Expression, int>(vars.Count);
            for (int i = 0; i < vars.Count; i++)
            {
                dict.Add(vars[i], i);
            }
            this.SelfVariable = Expression.Variable(typeof(object[]), null);
            this.Parent = parent;
            this.Variables = vars;
            this.Indexes = new ReadOnlyDictionary<Expression, int>(dict);
        }

        internal static object[] GetParent(object[] locals)
        {
            return ((StrongBox<object[]>) locals[0]).Value;
        }

        internal ParameterExpression ParentVariable
        {
            get
            {
                if (this.Parent == null)
                {
                    return null;
                }
                return this.Parent.SelfVariable;
            }
        }
    }
}

