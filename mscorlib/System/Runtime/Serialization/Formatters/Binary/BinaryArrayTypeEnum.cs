namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;

    [Serializable]
    internal enum BinaryArrayTypeEnum
    {
        Single,
        Jagged,
        Rectangular,
        SingleOffset,
        JaggedOffset,
        RectangularOffset
    }
}

