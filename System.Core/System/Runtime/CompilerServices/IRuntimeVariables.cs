namespace System.Runtime.CompilerServices
{
    using System;
    using System.Reflection;

    public interface IRuntimeVariables
    {
        int Count { get; }

        object this[int index] { get; set; }
    }
}

