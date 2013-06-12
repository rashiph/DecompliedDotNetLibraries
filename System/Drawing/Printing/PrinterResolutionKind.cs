namespace System.Drawing.Printing
{
    using System;

    [Serializable]
    public enum PrinterResolutionKind
    {
        Custom = 0,
        Draft = -1,
        High = -4,
        Low = -2,
        Medium = -3
    }
}

