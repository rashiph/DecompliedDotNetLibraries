namespace System.Globalization
{
    using System;

    [Serializable]
    internal enum BidiCategory
    {
        LeftToRight,
        LeftToRightEmbedding,
        LeftToRightOverride,
        RightToLeft,
        RightToLeftArabic,
        RightToLeftEmbedding,
        RightToLeftOverride,
        PopDirectionalFormat,
        EuropeanNumber,
        EuropeanNumberSeparator,
        EuropeanNumberTerminator,
        ArabicNumber,
        CommonNumberSeparator,
        NonSpacingMark,
        BoundaryNeutral,
        ParagraphSeparator,
        SegmentSeparator,
        Whitespace,
        OtherNeutrals
    }
}

