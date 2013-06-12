namespace System.ComponentModel.Design
{
    using System;

    public interface IDesignerHostTransactionState
    {
        bool IsClosingTransaction { get; }
    }
}

