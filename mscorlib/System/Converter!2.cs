namespace System
{
    using System.Runtime.CompilerServices;

    public delegate TOutput Converter<in TInput, out TOutput>(TInput input);
}

