namespace System.IO.Pipes
{
    using System;

    [Serializable, Flags]
    public enum PipeOptions
    {
        Asynchronous = 0x40000000,
        None = 0,
        WriteThrough = -2147483648
    }
}

