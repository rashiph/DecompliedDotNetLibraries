namespace System.Drawing.Imaging
{
    using System;

    [Flags]
    public enum ImageFlags
    {
        Caching = 0x20000,
        ColorSpaceCmyk = 0x20,
        ColorSpaceGray = 0x40,
        ColorSpaceRgb = 0x10,
        ColorSpaceYcbcr = 0x80,
        ColorSpaceYcck = 0x100,
        HasAlpha = 2,
        HasRealDpi = 0x1000,
        HasRealPixelSize = 0x2000,
        HasTranslucent = 4,
        None = 0,
        PartiallyScalable = 8,
        ReadOnly = 0x10000,
        Scalable = 1
    }
}

