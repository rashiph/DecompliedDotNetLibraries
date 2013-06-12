namespace System.Linq.Expressions.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class AnalyzedTree
    {
        internal readonly Dictionary<LambdaExpression, BoundConstants> Constants = new Dictionary<LambdaExpression, BoundConstants>();
        internal readonly Dictionary<object, CompilerScope> Scopes = new Dictionary<object, CompilerScope>();

        internal AnalyzedTree()
        {
        }

        internal System.Runtime.CompilerServices.DebugInfoGenerator DebugInfoGenerator { get; set; }
    }
}

