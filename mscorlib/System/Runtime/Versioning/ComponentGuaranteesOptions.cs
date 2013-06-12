namespace System.Runtime.Versioning
{
    using System;

    [Serializable, Flags]
    public enum ComponentGuaranteesOptions
    {
        Exchange = 1,
        None = 0,
        SideBySide = 4,
        Stable = 2
    }
}

