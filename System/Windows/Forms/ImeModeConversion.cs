namespace System.Windows.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ImeModeConversion
    {
        internal const int ImeDisabled = 1;
        internal const int ImeDirectInput = 2;
        internal const int ImeClosed = 3;
        internal const int ImeNativeInput = 4;
        internal const int ImeNativeFullHiragana = 4;
        internal const int ImeNativeHalfHiragana = 5;
        internal const int ImeNativeFullKatakana = 6;
        internal const int ImeNativeHalfKatakana = 7;
        internal const int ImeAlphaFull = 8;
        internal const int ImeAlphaHalf = 9;
        private static Dictionary<ImeMode, ImeModeConversion> imeModeConversionBits;
        internal int setBits;
        internal int clearBits;
        private static ImeMode[] japaneseTable;
        private static ImeMode[] koreanTable;
        private static ImeMode[] chineseTable;
        private static ImeMode[] unsupportedTable;
        internal static ImeMode[] ChineseTable
        {
            get
            {
                return chineseTable;
            }
        }
        internal static ImeMode[] JapaneseTable
        {
            get
            {
                return japaneseTable;
            }
        }
        internal static ImeMode[] KoreanTable
        {
            get
            {
                return koreanTable;
            }
        }
        internal static ImeMode[] UnsupportedTable
        {
            get
            {
                return unsupportedTable;
            }
        }
        internal static ImeMode[] InputLanguageTable
        {
            get
            {
                switch (((int) (((long) InputLanguage.CurrentInputLanguage.Handle) & 0xffffL)))
                {
                    case 0x411:
                        return japaneseTable;

                    case 0x412:
                    case 0x812:
                        return koreanTable;

                    case 0x804:
                    case 0x404:
                    case 0x1004:
                    case 0x1404:
                    case 0xc04:
                        return chineseTable;
                }
                return unsupportedTable;
            }
        }
        public static Dictionary<ImeMode, ImeModeConversion> ImeModeConversionBits
        {
            get
            {
                if (imeModeConversionBits == null)
                {
                    ImeModeConversion conversion;
                    imeModeConversionBits = new Dictionary<ImeMode, ImeModeConversion>(7);
                    conversion.setBits = 9;
                    conversion.clearBits = 2;
                    imeModeConversionBits.Add(ImeMode.Hiragana, conversion);
                    conversion.setBits = 11;
                    conversion.clearBits = 0;
                    imeModeConversionBits.Add(ImeMode.Katakana, conversion);
                    conversion.setBits = 3;
                    conversion.clearBits = 8;
                    imeModeConversionBits.Add(ImeMode.KatakanaHalf, conversion);
                    conversion.setBits = 8;
                    conversion.clearBits = 3;
                    imeModeConversionBits.Add(ImeMode.AlphaFull, conversion);
                    conversion.setBits = 0;
                    conversion.clearBits = 11;
                    imeModeConversionBits.Add(ImeMode.Alpha, conversion);
                    conversion.setBits = 9;
                    conversion.clearBits = 0;
                    imeModeConversionBits.Add(ImeMode.HangulFull, conversion);
                    conversion.setBits = 1;
                    conversion.clearBits = 8;
                    imeModeConversionBits.Add(ImeMode.Hangul, conversion);
                    conversion.setBits = 1;
                    conversion.clearBits = 10;
                    imeModeConversionBits.Add(ImeMode.OnHalf, conversion);
                }
                return imeModeConversionBits;
            }
        }
        public static bool IsCurrentConversionTableSupported
        {
            get
            {
                return (InputLanguageTable != UnsupportedTable);
            }
        }
        static ImeModeConversion()
        {
            japaneseTable = new ImeMode[] { ImeMode.Inherit, ImeMode.Disable, ImeMode.Off, ImeMode.Off, ImeMode.Hiragana, ImeMode.Hiragana, ImeMode.Katakana, ImeMode.KatakanaHalf, ImeMode.AlphaFull, ImeMode.Alpha };
            koreanTable = new ImeMode[] { ImeMode.Inherit, ImeMode.Disable, ImeMode.Alpha, ImeMode.Alpha, ImeMode.HangulFull, ImeMode.Hangul, ImeMode.HangulFull, ImeMode.Hangul, ImeMode.AlphaFull, ImeMode.Alpha };
            chineseTable = new ImeMode[] { ImeMode.Inherit, ImeMode.Disable, ImeMode.Off, ImeMode.Close, ImeMode.On, ImeMode.OnHalf, ImeMode.On, ImeMode.OnHalf, ImeMode.Off, ImeMode.Off };
            unsupportedTable = new ImeMode[0];
        }
    }
}

