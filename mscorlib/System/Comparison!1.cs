namespace System
{
    using System.Runtime.CompilerServices;

    public delegate int Comparison<in T>(T x, T y);
}

