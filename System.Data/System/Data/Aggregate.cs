namespace System.Data
{
    using System;

    internal enum Aggregate
    {
        Avg = 0x1f,
        Count = 0x22,
        Max = 0x21,
        Min = 0x20,
        None = -1,
        StDev = 0x23,
        Sum = 30,
        Var = 0x25
    }
}

