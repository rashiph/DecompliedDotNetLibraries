namespace System.Drawing.Printing
{
    using System;

    [Serializable]
    public enum PrintRange
    {
        AllPages = 0,
        CurrentPage = 0x400000,
        Selection = 1,
        SomePages = 2
    }
}

