namespace System.Linq.Expressions
{
    using System;

    internal interface IArgumentProvider
    {
        Expression GetArgument(int index);

        int ArgumentCount { get; }
    }
}

