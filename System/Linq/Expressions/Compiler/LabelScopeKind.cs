namespace System.Linq.Expressions.Compiler
{
    using System;

    internal enum LabelScopeKind
    {
        Statement,
        Block,
        Switch,
        Lambda,
        Try,
        Catch,
        Finally,
        Filter,
        Expression
    }
}

