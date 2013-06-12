namespace System.Text.RegularExpressions
{
    using System;

    [Flags]
    public enum RegexOptions
    {
        Compiled = 8,
        CultureInvariant = 0x200,
        ECMAScript = 0x100,
        ExplicitCapture = 4,
        IgnoreCase = 1,
        IgnorePatternWhitespace = 0x20,
        Multiline = 2,
        None = 0,
        RightToLeft = 0x40,
        Singleline = 0x10
    }
}

