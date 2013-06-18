namespace Microsoft.VisualBasic
{
    using System;

    [Flags]
    public enum VbStrConv
    {
        Hiragana = 0x20,
        Katakana = 0x10,
        LinguisticCasing = 0x400,
        Lowercase = 2,
        Narrow = 8,
        None = 0,
        ProperCase = 3,
        SimplifiedChinese = 0x100,
        TraditionalChinese = 0x200,
        Uppercase = 1,
        Wide = 4
    }
}

