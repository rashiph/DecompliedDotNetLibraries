namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class LikeOperator
    {
        private static string[] LigatureExpansions = new string[] { "", "ss", "sz", "AE", "ae", "TH", "th", "OE", "oe" };
        private static byte[] LigatureMap = new byte[0x8e];

        static LikeOperator()
        {
            LigatureMap[0x19] = 1;
            LigatureMap[0x19] = 2;
            LigatureMap[0] = 3;
            LigatureMap[0x20] = 4;
            LigatureMap[0x18] = 5;
            LigatureMap[0x38] = 6;
            LigatureMap[140] = 7;
            LigatureMap[0x8d] = 8;
        }

        private LikeOperator()
        {
        }

        private static void BuildPatternGroups(string Source, int SourceLength, ref int SourceIndex, LigatureInfo[] SourceLigatureInfo, string Pattern, int PatternLength, ref int PatternIndex, LigatureInfo[] PatternLigatureInfo, ref bool PatternError, ref int PGIndexForLastAsterisk, CompareInfo Comparer, CompareOptions Options, ref PatternGroup[] PatternGroups)
        {
            PatternError = false;
            PGIndexForLastAsterisk = 0;
            PatternGroups = new PatternGroup[0x10];
            int num3 = 15;
            PatternType nONE = PatternType.NONE;
            int index = 0;
            do
            {
                PatternGroup[] groupArray2;
                int num6;
                if (index >= num3)
                {
                    PatternGroup[] array = new PatternGroup[(num3 + 0x10) + 1];
                    PatternGroups.CopyTo(array, 0);
                    PatternGroups = array;
                    num3 += 0x10;
                }
                switch (Pattern[PatternIndex])
                {
                    case '*':
                    case 0xff0a:
                        if (nONE != PatternType.STAR)
                        {
                            nONE = PatternType.STAR;
                            PatternGroups[index].PatType = PatternType.STAR;
                            PGIndexForLastAsterisk = index;
                            index++;
                        }
                        break;

                    case '[':
                    case 0xff3b:
                    {
                        bool seenNot = false;
                        List<Range> rangeList = new List<Range>();
                        if (!ValidateRangePattern(Pattern, PatternLength, ref PatternIndex, PatternLigatureInfo, Comparer, Options, ref seenNot, ref rangeList))
                        {
                            PatternError = true;
                            return;
                        }
                        if (rangeList.Count != 0)
                        {
                            if (seenNot)
                            {
                                nONE = PatternType.EXCLIST;
                            }
                            else
                            {
                                nONE = PatternType.INCLIST;
                            }
                            PatternGroups[index].PatType = nONE;
                            PatternGroups[index].CharCount = 1;
                            PatternGroups[index].RangeList = rangeList;
                            index++;
                        }
                        break;
                    }
                    case '#':
                    case 0xff03:
                        if (nONE == PatternType.DIGIT)
                        {
                            groupArray2 = PatternGroups;
                            num6 = index - 1;
                            groupArray2[num6].CharCount++;
                        }
                        else
                        {
                            PatternGroups[index].PatType = PatternType.DIGIT;
                            PatternGroups[index].CharCount = 1;
                            index++;
                            nONE = PatternType.DIGIT;
                        }
                        break;

                    case '?':
                    case 0xff1f:
                        if (nONE == PatternType.ANYCHAR)
                        {
                            groupArray2 = PatternGroups;
                            num6 = index - 1;
                            groupArray2[num6].CharCount++;
                        }
                        else
                        {
                            PatternGroups[index].PatType = PatternType.ANYCHAR;
                            PatternGroups[index].CharCount = 1;
                            index++;
                            nONE = PatternType.ANYCHAR;
                        }
                        break;

                    default:
                    {
                        int num5 = PatternIndex;
                        int num4 = PatternIndex;
                        if (num4 >= PatternLength)
                        {
                            num4 = PatternLength - 1;
                        }
                        if (nONE == PatternType.STRING)
                        {
                            groupArray2 = PatternGroups;
                            num6 = index - 1;
                            groupArray2[num6].CharCount++;
                            PatternGroups[index - 1].StringPatternEnd = num4;
                        }
                        else
                        {
                            PatternGroups[index].PatType = PatternType.STRING;
                            PatternGroups[index].CharCount = 1;
                            PatternGroups[index].StringPatternStart = num5;
                            PatternGroups[index].StringPatternEnd = num4;
                            index++;
                            nONE = PatternType.STRING;
                        }
                        break;
                    }
                }
                PatternIndex++;
            }
            while (PatternIndex < PatternLength);
            PatternGroups[index].PatType = PatternType.NONE;
            PatternGroups[index].MinSourceIndex = SourceLength;
            int num = SourceLength;
            while (index > 0)
            {
                switch (PatternGroups[index].PatType)
                {
                    case PatternType.STRING:
                        num -= PatternGroups[index].CharCount;
                        break;

                    case PatternType.EXCLIST:
                    case PatternType.INCLIST:
                        num--;
                        break;

                    case PatternType.DIGIT:
                    case PatternType.ANYCHAR:
                        num -= PatternGroups[index].CharCount;
                        break;
                }
                PatternGroups[index].MaxSourceIndex = num;
                index--;
            }
        }

        private static int CanCharExpand(char ch, byte[] LocaleSpecificLigatureTable, CompareInfo Comparer, CompareOptions Options)
        {
            int num;
            byte index = LigatureIndex(ch);
            if (index == 0)
            {
                return 0;
            }
            if (LocaleSpecificLigatureTable[index] == 0)
            {
                if (Comparer.Compare(Conversions.ToString(ch), LigatureExpansions[index]) == 0)
                {
                    LocaleSpecificLigatureTable[index] = 1;
                }
                else
                {
                    LocaleSpecificLigatureTable[index] = 2;
                }
            }
            if (LocaleSpecificLigatureTable[index] == 1)
            {
                return index;
            }
            return num;
        }

        private static int CompareChars(char Left, char Right, CompareInfo Comparer, CompareOptions Options)
        {
            if (Options == CompareOptions.Ordinal)
            {
                return (Left - Right);
            }
            return Comparer.Compare(Conversions.ToString(Left), Conversions.ToString(Right), Options);
        }

        private static int CompareChars(string Left, string Right, CompareInfo Comparer, CompareOptions Options)
        {
            if (Options == CompareOptions.Ordinal)
            {
                return (Left[0] - Right[0]);
            }
            return Comparer.Compare(Left, Right, Options);
        }

        private static int CompareChars(string Left, int LeftLength, int LeftStart, ref int LeftEnd, LigatureInfo[] LeftLigatureInfo, string Right, int RightLength, int RightStart, ref int RightEnd, LigatureInfo[] RightLigatureInfo, CompareInfo Comparer, CompareOptions Options, bool MatchBothCharsOfExpandedCharInRight = false, bool UseUnexpandedCharForRight = false)
        {
            LeftEnd = LeftStart;
            RightEnd = RightStart;
            if (Options == CompareOptions.Ordinal)
            {
                return (Left[LeftStart] - Right[RightStart]);
            }
            if (UseUnexpandedCharForRight)
            {
                if ((RightLigatureInfo != null) && (RightLigatureInfo[RightEnd].Kind == CharKind.ExpandedChar1))
                {
                    Right = Right.Substring(RightStart, RightEnd - RightStart);
                    Right = Right + Conversions.ToString(RightLigatureInfo[RightEnd].CharBeforeExpansion);
                    RightEnd++;
                    return CompareChars(Left.Substring(LeftStart, (LeftEnd - LeftStart) + 1), Right, Comparer, Options);
                }
            }
            else if (MatchBothCharsOfExpandedCharInRight)
            {
                int num2 = RightEnd;
                SkipToEndOfExpandedChar(RightLigatureInfo, RightLength, ref RightEnd);
                if (num2 < RightEnd)
                {
                    int num4 = 0;
                    if ((LeftEnd + 1) < LeftLength)
                    {
                        num4 = 1;
                    }
                    int num3 = CompareChars(Left.Substring(LeftStart, ((LeftEnd - LeftStart) + 1) + num4), Right.Substring(RightStart, (RightEnd - RightStart) + 1), Comparer, Options);
                    if (num3 == 0)
                    {
                        LeftEnd += num4;
                    }
                    return num3;
                }
            }
            if ((LeftEnd == LeftStart) && (RightEnd == RightStart))
            {
                return Comparer.Compare(Conversions.ToString(Left[LeftStart]), Conversions.ToString(Right[RightStart]), Options);
            }
            return CompareChars(Left.Substring(LeftStart, (LeftEnd - LeftStart) + 1), Right.Substring(RightStart, (RightEnd - RightStart) + 1), Comparer, Options);
        }

        private static void ExpandString(ref string Input, ref int Length, ref LigatureInfo[] InputLigatureInfo, byte[] LocaleSpecificLigatureTable, CompareInfo Comparer, CompareOptions Options, ref bool WidthChanged, bool UseFullWidth)
        {
            WidthChanged = false;
            if (Length != 0)
            {
                int num;
                CultureInfo cultureInfo = Utils.GetCultureInfo();
                Encoding encoding = Encoding.GetEncoding(cultureInfo.TextInfo.ANSICodePage);
                int dwMapFlags = 0x100;
                bool flag = false;
                if (!encoding.IsSingleByte)
                {
                    dwMapFlags = 0x400100;
                    if (Strings.IsValidCodePage(0x3a4))
                    {
                        if (UseFullWidth)
                        {
                            dwMapFlags = 0xa00100;
                        }
                        else
                        {
                            dwMapFlags = 0x600100;
                        }
                        Input = Strings.vbLCMapString(cultureInfo, dwMapFlags, Input);
                        flag = true;
                        if (Input.Length != Length)
                        {
                            Length = Input.Length;
                            WidthChanged = true;
                        }
                    }
                }
                if (!flag)
                {
                    Input = Strings.vbLCMapString(cultureInfo, dwMapFlags, Input);
                }
                int num6 = Length - 1;
                for (int i = 0; i <= num6; i++)
                {
                    char ch = Input[i];
                    if (CanCharExpand(ch, LocaleSpecificLigatureTable, Comparer, Options) != 0)
                    {
                        num++;
                    }
                }
                if (num > 0)
                {
                    InputLigatureInfo = new LigatureInfo[((Length + num) - 1) + 1];
                    StringBuilder builder = new StringBuilder((Length + num) - 1);
                    int index = 0;
                    int num7 = Length - 1;
                    for (int j = 0; j <= num7; j++)
                    {
                        char ch2 = Input[j];
                        if (CanCharExpand(ch2, LocaleSpecificLigatureTable, Comparer, Options) != 0)
                        {
                            string str = GetCharExpansion(ch2, LocaleSpecificLigatureTable, Comparer, Options);
                            builder.Append(str);
                            InputLigatureInfo[index].Kind = CharKind.ExpandedChar1;
                            InputLigatureInfo[index].CharBeforeExpansion = ch2;
                            index++;
                            InputLigatureInfo[index].Kind = CharKind.ExpandedChar2;
                            InputLigatureInfo[index].CharBeforeExpansion = ch2;
                        }
                        else
                        {
                            builder.Append(ch2);
                        }
                        index++;
                    }
                    Input = builder.ToString();
                    Length = builder.Length;
                }
            }
        }

        private static string GetCharExpansion(char ch, byte[] LocaleSpecificLigatureTable, CompareInfo Comparer, CompareOptions Options)
        {
            int index = CanCharExpand(ch, LocaleSpecificLigatureTable, Comparer, Options);
            if (index == 0)
            {
                return Conversions.ToString(ch);
            }
            return LigatureExpansions[index];
        }

        private static byte LigatureIndex(char ch)
        {
            if ((Strings.Asc(ch) >= 0xc6) && (Strings.Asc(ch) <= 0x153))
            {
                return LigatureMap[Strings.Asc(ch) - 0xc6];
            }
            return 0;
        }

        public static object LikeObject(object Source, object Pattern, CompareMethod CompareOption)
        {
            TypeCode empty;
            TypeCode typeCode;
            IConvertible convertible = Source as IConvertible;
            if (convertible == null)
            {
                if (Source == null)
                {
                    empty = TypeCode.Empty;
                }
                else
                {
                    empty = TypeCode.Object;
                }
            }
            else
            {
                empty = convertible.GetTypeCode();
            }
            IConvertible convertible2 = Pattern as IConvertible;
            if (convertible2 == null)
            {
                if (Pattern == null)
                {
                    typeCode = TypeCode.Empty;
                }
                else
                {
                    typeCode = TypeCode.Object;
                }
            }
            else
            {
                typeCode = convertible2.GetTypeCode();
            }
            if ((empty == TypeCode.Object) && (Source is char[]))
            {
                empty = TypeCode.String;
            }
            if ((typeCode == TypeCode.Object) && (Pattern is char[]))
            {
                typeCode = TypeCode.String;
            }
            if ((empty != TypeCode.Object) && (typeCode != TypeCode.Object))
            {
                return LikeString(Conversions.ToString(Source), Conversions.ToString(Pattern), CompareOption);
            }
            return Operators.InvokeUserDefinedOperator(Symbols.UserDefinedOperator.Like, new object[] { Source, Pattern });
        }

        public static bool LikeString(string Source, string Pattern, CompareMethod CompareOption)
        {
            CompareInfo compareInfo;
            CompareOptions ordinal;
            char ch;
            int num;
            int length;
            LigatureInfo[] inputLigatureInfo = null;
            int num3;
            int num4;
            LigatureInfo[] infoArray2 = null;
            bool flag7;
            if (Pattern == null)
            {
                length = 0;
            }
            else
            {
                length = Pattern.Length;
            }
            if (Source == null)
            {
                num4 = 0;
            }
            else
            {
                num4 = Source.Length;
            }
            if (CompareOption == CompareMethod.Binary)
            {
                ordinal = CompareOptions.Ordinal;
                compareInfo = null;
            }
            else
            {
                compareInfo = Utils.GetCultureInfo().CompareInfo;
                ordinal = CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase;
                byte[] localeSpecificLigatureTable = new byte[(LigatureExpansions.Length - 1) + 1];
                flag7 = false;
                ExpandString(ref Source, ref num4, ref infoArray2, localeSpecificLigatureTable, compareInfo, ordinal, ref flag7, false);
                flag7 = false;
                ExpandString(ref Pattern, ref length, ref inputLigatureInfo, localeSpecificLigatureTable, compareInfo, ordinal, ref flag7, false);
            }
            while ((num < length) && (num3 < num4))
            {
                ch = Pattern[num];
                switch (ch)
                {
                    case '?':
                    case 0xff1f:
                        SkipToEndOfExpandedChar(infoArray2, num4, ref num3);
                        break;

                    case '#':
                    case 0xff03:
                        if (!char.IsDigit(Source[num3]))
                        {
                            return false;
                        }
                        break;

                    case '[':
                    case 0xff3b:
                    {
                        bool flag2;
                        bool flag3;
                        bool flag4;
                        flag7 = false;
                        MatchRange(Source, num4, ref num3, infoArray2, Pattern, length, ref num, inputLigatureInfo, ref flag3, ref flag2, ref flag4, compareInfo, ordinal, ref flag7, null, false);
                        if (flag4)
                        {
                            throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Pattern" }));
                        }
                        if (flag2)
                        {
                            return false;
                        }
                        if (!flag3)
                        {
                            break;
                        }
                        num++;
                        continue;
                    }
                    case '*':
                    case 0xff0a:
                        bool flag5;
                        bool flag6;
                        MatchAsterisk(Source, num4, num3, infoArray2, Pattern, length, num, inputLigatureInfo, ref flag5, ref flag6, compareInfo, ordinal);
                        if (flag6)
                        {
                            throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Pattern" }));
                        }
                        return !flag5;

                    default:
                        if (CompareChars(Source, num4, num3, ref num3, infoArray2, Pattern, length, num, ref num, inputLigatureInfo, compareInfo, ordinal, false, false) != 0)
                        {
                            return false;
                        }
                        break;
                }
                num++;
                num3++;
            }
            while (num < length)
            {
                ch = Pattern[num];
                if ((ch == '*') || (ch == 0xff0a))
                {
                    num++;
                }
                else
                {
                    if (((num + 1) >= length) || (((ch != '[') || (Pattern[num + 1] != ']')) && ((ch != 0xff3b) || (Pattern[num + 1] != 0xff3d))))
                    {
                        break;
                    }
                    num += 2;
                }
            }
            return ((num >= length) && (num3 >= num4));
        }

        private static void MatchAsterisk(string Source, int SourceLength, int SourceIndex, LigatureInfo[] SourceLigatureInfo, string Pattern, int PatternLength, int PatternIndex, LigatureInfo[] PattternLigatureInfo, ref bool Mismatch, ref bool PatternError, CompareInfo Comparer, CompareOptions Options)
        {
            Mismatch = false;
            PatternError = false;
            if (PatternIndex < PatternLength)
            {
                int num2;
                PatternGroup[] patternGroups = null;
                BuildPatternGroups(Source, SourceLength, ref SourceIndex, SourceLigatureInfo, Pattern, PatternLength, ref PatternIndex, PattternLigatureInfo, ref PatternError, ref num2, Comparer, Options, ref patternGroups);
                if (!PatternError)
                {
                    if (patternGroups[num2 + 1].PatType != PatternType.NONE)
                    {
                        int num4;
                        int num5 = SourceIndex;
                        int index = num2 + 1;
                        do
                        {
                            num4 += patternGroups[index].CharCount;
                            index++;
                        }
                        while (patternGroups[index].PatType != PatternType.NONE);
                        SourceIndex = SourceLength;
                        SubtractChars(Source, SourceLength, ref SourceIndex, num4, SourceLigatureInfo, Options);
                        MatchAsterisk(Source, SourceLength, SourceIndex, SourceLigatureInfo, Pattern, PattternLigatureInfo, patternGroups, num2, ref Mismatch, ref PatternError, Comparer, Options);
                        if (PatternError)
                        {
                            return;
                        }
                        if (Mismatch)
                        {
                            return;
                        }
                        SourceLength = patternGroups[num2 + 1].StartIndexOfPossibleMatch;
                        if (SourceLength <= 0)
                        {
                            return;
                        }
                        patternGroups[index].MaxSourceIndex = SourceLength;
                        patternGroups[index].MinSourceIndex = SourceLength;
                        patternGroups[index].StartIndexOfPossibleMatch = 0;
                        patternGroups[num2 + 1] = patternGroups[index];
                        patternGroups[num2].MinSourceIndex = 0;
                        patternGroups[num2].StartIndexOfPossibleMatch = 0;
                        index = num2 + 1;
                        int num3 = SourceLength;
                        while (index > 0)
                        {
                            switch (patternGroups[index].PatType)
                            {
                                case PatternType.STRING:
                                    num3 -= patternGroups[index].CharCount;
                                    break;

                                case PatternType.EXCLIST:
                                case PatternType.INCLIST:
                                    num3--;
                                    break;

                                case PatternType.DIGIT:
                                case PatternType.ANYCHAR:
                                    num3 -= patternGroups[index].CharCount;
                                    break;
                            }
                            patternGroups[index].MaxSourceIndex = num3;
                            index--;
                        }
                        SourceIndex = num5;
                    }
                    MatchAsterisk(Source, SourceLength, SourceIndex, SourceLigatureInfo, Pattern, PattternLigatureInfo, patternGroups, 0, ref Mismatch, ref PatternError, Comparer, Options);
                }
            }
        }

        private static void MatchAsterisk(string Source, int SourceLength, int SourceIndex, LigatureInfo[] SourceLigatureInfo, string Pattern, LigatureInfo[] PatternLigatureInfo, PatternGroup[] PatternGroups, int PGIndex, ref bool Mismatch, ref bool PatternError, CompareInfo Comparer, CompareOptions Options)
        {
            PatternGroup group;
            int index = PGIndex;
            int maxSourceIndex = SourceIndex;
            int num3 = -1;
            int num2 = -1;
            PatternGroups[PGIndex].MinSourceIndex = SourceIndex;
            PatternGroups[PGIndex].StartIndexOfPossibleMatch = SourceIndex;
            PGIndex++;
        Label_002D:
            group = PatternGroups[PGIndex];
            switch (group.PatType)
            {
                case PatternType.STRING:
                {
                Label_006A:
                    if (SourceIndex > group.MaxSourceIndex)
                    {
                        Mismatch = true;
                        return;
                    }
                    PatternGroups[PGIndex].StartIndexOfPossibleMatch = SourceIndex;
                    int stringPatternStart = group.StringPatternStart;
                    int num6 = 0;
                    int leftStart = SourceIndex;
                    bool flag = true;
                    do
                    {
                        int num8 = CompareChars(Source, SourceLength, leftStart, ref leftStart, SourceLigatureInfo, Pattern, group.StringPatternEnd + 1, stringPatternStart, ref stringPatternStart, PatternLigatureInfo, Comparer, Options, false, false);
                        if (flag)
                        {
                            flag = false;
                            num6 = leftStart + 1;
                        }
                        if (num8 != 0)
                        {
                            SourceIndex = num6;
                            index = PGIndex - 1;
                            maxSourceIndex = SourceIndex;
                            goto Label_006A;
                        }
                        stringPatternStart++;
                        leftStart++;
                        if (stringPatternStart > group.StringPatternEnd)
                        {
                            SourceIndex = leftStart;
                            goto Label_02F4;
                        }
                    }
                    while (leftStart < SourceLength);
                    Mismatch = true;
                    return;
                }
                case PatternType.EXCLIST:
                case PatternType.INCLIST:
                {
                    while (true)
                    {
                        if (SourceIndex > group.MaxSourceIndex)
                        {
                            Mismatch = true;
                            return;
                        }
                        PatternGroups[PGIndex].StartIndexOfPossibleMatch = SourceIndex;
                        if (MatchRangeAfterAsterisk(Source, SourceLength, ref SourceIndex, SourceLigatureInfo, Pattern, PatternLigatureInfo, group, Comparer, Options))
                        {
                            goto Label_02F4;
                        }
                        index = PGIndex - 1;
                        maxSourceIndex = SourceIndex;
                    }
                }
                case PatternType.DIGIT:
                {
                Label_010A:
                    if (SourceIndex > group.MaxSourceIndex)
                    {
                        Mismatch = true;
                        return;
                    }
                    PatternGroups[PGIndex].StartIndexOfPossibleMatch = SourceIndex;
                    int num11 = group.CharCount;
                    for (int j = 1; j <= num11; j++)
                    {
                        char c = Source[SourceIndex];
                        SourceIndex++;
                        if (!char.IsDigit(c))
                        {
                            index = PGIndex - 1;
                            maxSourceIndex = SourceIndex;
                            goto Label_010A;
                        }
                    }
                    goto Label_02F4;
                }
                case PatternType.ANYCHAR:
                    if (SourceIndex <= group.MaxSourceIndex)
                    {
                        break;
                    }
                    Mismatch = true;
                    return;

                case PatternType.STAR:
                    PatternGroups[PGIndex].StartIndexOfPossibleMatch = SourceIndex;
                    group.MinSourceIndex = SourceIndex;
                    if (PatternGroups[index].PatType == PatternType.STAR)
                    {
                        goto Label_02E9;
                    }
                    if (SourceIndex <= group.MaxSourceIndex)
                    {
                        goto Label_0285;
                    }
                    Mismatch = true;
                    return;

                case PatternType.NONE:
                    PatternGroups[PGIndex].StartIndexOfPossibleMatch = group.MaxSourceIndex;
                    if (SourceIndex < group.MaxSourceIndex)
                    {
                        index = PGIndex - 1;
                        maxSourceIndex = group.MaxSourceIndex;
                    }
                    if ((PatternGroups[index].PatType == PatternType.STAR) || (PatternGroups[index].PatType == PatternType.NONE))
                    {
                        return;
                    }
                    goto Label_0285;

                default:
                    goto Label_02F4;
            }
            PatternGroups[PGIndex].StartIndexOfPossibleMatch = SourceIndex;
            int charCount = group.CharCount;
            for (int i = 1; i <= charCount; i++)
            {
                if (SourceIndex >= SourceLength)
                {
                    Mismatch = true;
                    return;
                }
                SkipToEndOfExpandedChar(SourceLigatureInfo, SourceLength, ref SourceIndex);
                SourceIndex++;
            }
            goto Label_02F4;
        Label_0285:
            num3 = PGIndex;
            SourceIndex = maxSourceIndex;
            PGIndex = index;
            do
            {
                SubtractChars(Source, SourceLength, ref SourceIndex, PatternGroups[PGIndex].CharCount, SourceLigatureInfo, Options);
                PGIndex--;
            }
            while (PatternGroups[PGIndex].PatType != PatternType.STAR);
            SourceIndex = Math.Max(SourceIndex, PatternGroups[PGIndex].MinSourceIndex + 1);
            PatternGroups[PGIndex].MinSourceIndex = SourceIndex;
            num2 = PGIndex;
        Label_02E9:
            PGIndex++;
            goto Label_002D;
        Label_02F4:
            if (PGIndex == index)
            {
                if (SourceIndex == maxSourceIndex)
                {
                    SourceIndex = PatternGroups[num3].MinSourceIndex;
                    PGIndex = num3;
                    index = num3;
                }
                else if (SourceIndex < maxSourceIndex)
                {
                    PatternGroup[] groupArray = PatternGroups;
                    int num13 = num2;
                    groupArray[num13].MinSourceIndex++;
                    SourceIndex = PatternGroups[num2].MinSourceIndex;
                    PGIndex = num2 + 1;
                }
                else
                {
                    PGIndex++;
                    index = num2;
                }
            }
            else
            {
                PGIndex++;
            }
            goto Label_002D;
        }

        private static void MatchRange(string Source, int SourceLength, ref int SourceIndex, LigatureInfo[] SourceLigatureInfo, string Pattern, int PatternLength, ref int PatternIndex, LigatureInfo[] PatternLigatureInfo, ref bool RangePatternEmpty, ref bool Mismatch, ref bool PatternError, CompareInfo Comparer, CompareOptions Options, ref bool SeenNot = false, List<Range> RangeList = null, bool ValidatePatternWithoutMatching = false)
        {
            Range range;
            string str2;
            int num2;
            int num3;
            int num5;
            RangePatternEmpty = false;
            Mismatch = false;
            PatternError = false;
            SeenNot = false;
            PatternIndex++;
            if (PatternIndex >= PatternLength)
            {
                PatternError = true;
                return;
            }
            char ch = Pattern[PatternIndex];
            switch (ch)
            {
                case '!':
                case 0xff01:
                    SeenNot = true;
                    PatternIndex++;
                    if (PatternIndex >= PatternLength)
                    {
                        Mismatch = true;
                        return;
                    }
                    ch = Pattern[PatternIndex];
                    break;
            }
            if ((ch == ']') || (ch == 0xff3d))
            {
                if (SeenNot)
                {
                    SeenNot = false;
                    if (!ValidatePatternWithoutMatching)
                    {
                        Mismatch = CompareChars(Source[SourceIndex], '!', Comparer, Options) != 0;
                    }
                    if (RangeList != null)
                    {
                        range.Start = PatternIndex - 1;
                        range.StartLength = 1;
                        range.End = -1;
                        range.EndLength = 0;
                        RangeList.Add(range);
                    }
                    return;
                }
                RangePatternEmpty = true;
                return;
            }
        Label_00CE:
            str2 = null;
            string right = null;
            if ((ch == ']') || (ch == 0xff3d))
            {
                Mismatch = !SeenNot;
                return;
            }
            if ((!ValidatePatternWithoutMatching && (PatternLigatureInfo != null)) && (PatternLigatureInfo[PatternIndex].Kind == CharKind.ExpandedChar1))
            {
                if (CompareChars(Source, SourceLength, SourceIndex, ref num3, SourceLigatureInfo, Pattern, PatternLength, PatternIndex, ref num2, PatternLigatureInfo, Comparer, Options, true, false) != 0)
                {
                    goto Label_0145;
                }
                SourceIndex = num3;
                PatternIndex = num2;
                goto Label_037F;
            }
            num2 = PatternIndex;
            SkipToEndOfExpandedChar(PatternLigatureInfo, PatternLength, ref num2);
        Label_0145:
            range.Start = PatternIndex;
            range.StartLength = (num2 - PatternIndex) + 1;
            if (Options == CompareOptions.Ordinal)
            {
                str2 = Conversions.ToString(Pattern[PatternIndex]);
            }
            else if ((PatternLigatureInfo != null) && (PatternLigatureInfo[PatternIndex].Kind == CharKind.ExpandedChar1))
            {
                str2 = Conversions.ToString(PatternLigatureInfo[PatternIndex].CharBeforeExpansion);
                PatternIndex = num2;
            }
            else
            {
                str2 = Pattern.Substring(PatternIndex, (num2 - PatternIndex) + 1);
                PatternIndex = num2;
            }
            if ((((num2 + 2) >= PatternLength) || ((Pattern[num2 + 1] != '-') && (Pattern[num2 + 1] != 0xff0d))) || ((Pattern[num2 + 2] == ']') || (Pattern[num2 + 2] == 0xff3d)))
            {
                if (!ValidatePatternWithoutMatching)
                {
                    num5 = 0;
                    if (CompareChars(Source, SourceLength, SourceIndex, ref num3, SourceLigatureInfo, Pattern, range.Start + range.StartLength, range.Start, ref num5, PatternLigatureInfo, Comparer, Options, false, true) == 0)
                    {
                        goto Label_037F;
                    }
                }
                range.End = -1;
                range.EndLength = 0;
                goto Label_0409;
            }
            PatternIndex += 2;
            if ((!ValidatePatternWithoutMatching && (PatternLigatureInfo != null)) && (PatternLigatureInfo[PatternIndex].Kind == CharKind.ExpandedChar1))
            {
                if (CompareChars(Source, SourceLength, SourceIndex, ref num3, SourceLigatureInfo, Pattern, PatternLength, PatternIndex, ref num2, PatternLigatureInfo, Comparer, Options, true, false) != 0)
                {
                    goto Label_0279;
                }
                PatternIndex = num2;
                goto Label_037F;
            }
            num2 = PatternIndex;
            SkipToEndOfExpandedChar(PatternLigatureInfo, PatternLength, ref num2);
        Label_0279:
            range.End = PatternIndex;
            range.EndLength = (num2 - PatternIndex) + 1;
            if (Options == CompareOptions.Ordinal)
            {
                right = Conversions.ToString(Pattern[PatternIndex]);
            }
            else if ((PatternLigatureInfo != null) && (PatternLigatureInfo[PatternIndex].Kind == CharKind.ExpandedChar1))
            {
                right = Conversions.ToString(PatternLigatureInfo[PatternIndex].CharBeforeExpansion);
                PatternIndex = num2;
            }
            else
            {
                right = Pattern.Substring(PatternIndex, (num2 - PatternIndex) + 1);
                PatternIndex = num2;
            }
            if (CompareChars(str2, right, Comparer, Options) > 0)
            {
                PatternError = true;
                return;
            }
            if (ValidatePatternWithoutMatching)
            {
                goto Label_0409;
            }
            int rightEnd = 0;
            if (CompareChars(Source, SourceLength, SourceIndex, ref num3, SourceLigatureInfo, Pattern, range.Start + range.StartLength, range.Start, ref rightEnd, PatternLigatureInfo, Comparer, Options, false, true) < 0)
            {
                goto Label_0409;
            }
            num5 = 0;
            if (CompareChars(Source, SourceLength, SourceIndex, ref num3, SourceLigatureInfo, Pattern, range.End + range.EndLength, range.End, ref num5, PatternLigatureInfo, Comparer, Options, false, true) > 0)
            {
                goto Label_0409;
            }
        Label_037F:
            if (SeenNot)
            {
                Mismatch = true;
                return;
            }
            do
            {
                PatternIndex++;
                if (PatternIndex >= PatternLength)
                {
                    PatternError = true;
                    return;
                }
            }
            while ((Pattern[PatternIndex] != ']') && (Pattern[PatternIndex] != 0xff3d));
            SourceIndex = num3;
            return;
        Label_0409:
            if (RangeList != null)
            {
                RangeList.Add(range);
            }
            PatternIndex++;
            if (PatternIndex >= PatternLength)
            {
                PatternError = true;
            }
            else
            {
                ch = Pattern[PatternIndex];
                goto Label_00CE;
            }
        }

        private static bool MatchRangeAfterAsterisk(string Source, int SourceLength, ref int SourceIndex, LigatureInfo[] SourceLigatureInfo, string Pattern, LigatureInfo[] PatternLigatureInfo, PatternGroup PG, CompareInfo Comparer, CompareOptions Options)
        {
            List<Range> rangeList = PG.RangeList;
            int leftEnd = SourceIndex;
            bool flag = false;
            foreach (Range range in rangeList)
            {
                int num4;
                int num2 = 1;
                if ((PatternLigatureInfo != null) && (PatternLigatureInfo[range.Start].Kind == CharKind.ExpandedChar1))
                {
                    num4 = 0;
                    if (CompareChars(Source, SourceLength, SourceIndex, ref leftEnd, SourceLigatureInfo, Pattern, range.Start + range.StartLength, range.Start, ref num4, PatternLigatureInfo, Comparer, Options, true, false) == 0)
                    {
                        flag = true;
                        break;
                    }
                }
                num4 = 0;
                int num3 = CompareChars(Source, SourceLength, SourceIndex, ref leftEnd, SourceLigatureInfo, Pattern, range.Start + range.StartLength, range.Start, ref num4, PatternLigatureInfo, Comparer, Options, false, true);
                if ((num3 > 0) && (range.End >= 0))
                {
                    num4 = 0;
                    num2 = CompareChars(Source, SourceLength, SourceIndex, ref leftEnd, SourceLigatureInfo, Pattern, range.End + range.EndLength, range.End, ref num4, PatternLigatureInfo, Comparer, Options, false, true);
                }
                if ((num3 == 0) || ((num3 > 0) && (num2 <= 0)))
                {
                    flag = true;
                    break;
                }
            }
            if (PG.PatType == PatternType.EXCLIST)
            {
                flag = !flag;
            }
            SourceIndex = leftEnd + 1;
            return flag;
        }

        private static void SkipToEndOfExpandedChar(LigatureInfo[] InputLigatureInfo, int Length, ref int Current)
        {
            if (((InputLigatureInfo != null) && (Current < Length)) && (InputLigatureInfo[Current].Kind == CharKind.ExpandedChar1))
            {
                Current++;
            }
        }

        private static void SubtractChars(string Input, int InputLength, ref int Current, int CharsToSubtract, LigatureInfo[] InputLigatureInfo, CompareOptions Options)
        {
            if (Options == CompareOptions.Ordinal)
            {
                Current -= CharsToSubtract;
                if (Current < 0)
                {
                    Current = 0;
                }
            }
            else
            {
                int num2 = CharsToSubtract;
                for (int i = 1; i <= num2; i++)
                {
                    SubtractOneCharInTextCompareMode(Input, InputLength, ref Current, InputLigatureInfo, Options);
                    if (Current < 0)
                    {
                        Current = 0;
                        break;
                    }
                }
            }
        }

        private static void SubtractOneCharInTextCompareMode(string Input, int InputLength, ref int Current, LigatureInfo[] InputLigatureInfo, CompareOptions Options)
        {
            if (Current >= InputLength)
            {
                Current--;
            }
            else if ((InputLigatureInfo != null) && (InputLigatureInfo[Current].Kind == CharKind.ExpandedChar2))
            {
                Current -= 2;
            }
            else
            {
                Current--;
            }
        }

        private static bool ValidateRangePattern(string Pattern, int PatternLength, ref int PatternIndex, LigatureInfo[] PatternLigatureInfo, CompareInfo Comparer, CompareOptions Options, ref bool SeenNot, ref List<Range> RangeList)
        {
            bool flag;
            int sourceIndex = -1;
            bool rangePatternEmpty = false;
            bool mismatch = false;
            MatchRange(null, -1, ref sourceIndex, null, Pattern, PatternLength, ref PatternIndex, PatternLigatureInfo, ref rangePatternEmpty, ref mismatch, ref flag, Comparer, Options, ref SeenNot, RangeList, true);
            return !flag;
        }

        private enum CharKind
        {
            None,
            ExpandedChar1,
            ExpandedChar2
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LigatureInfo
        {
            internal LikeOperator.CharKind Kind;
            internal char CharBeforeExpansion;
        }

        private enum Ligatures
        {
            ae = 230,
            aeUpper = 0xc6,
            Invalid = 0,
            Max = 0x153,
            Min = 0xc6,
            oe = 0x153,
            oeUpper = 0x152,
            ssBeta = 0xdf,
            szBeta = 0xdf,
            th = 0xfe,
            thUpper = 0xde
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PatternGroup
        {
            internal LikeOperator.PatternType PatType;
            internal int MaxSourceIndex;
            internal int CharCount;
            internal int StringPatternStart;
            internal int StringPatternEnd;
            internal int MinSourceIndex;
            internal List<LikeOperator.Range> RangeList;
            public int StartIndexOfPossibleMatch;
        }

        private enum PatternType
        {
            STRING,
            EXCLIST,
            INCLIST,
            DIGIT,
            ANYCHAR,
            STAR,
            NONE
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Range
        {
            internal int Start;
            internal int StartLength;
            internal int End;
            internal int EndLength;
        }
    }
}

