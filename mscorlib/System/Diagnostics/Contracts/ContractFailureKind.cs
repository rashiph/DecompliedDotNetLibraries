namespace System.Diagnostics.Contracts
{
    using System;

    public enum ContractFailureKind
    {
        Precondition,
        Postcondition,
        PostconditionOnException,
        Invariant,
        Assert,
        Assume
    }
}

