namespace System.Data
{
    using System;

    internal enum Nodes
    {
        Noop,
        Unop,
        UnopSpec,
        Binop,
        BinopSpec,
        Zop,
        Call,
        Const,
        Name,
        Paren,
        Conv
    }
}

