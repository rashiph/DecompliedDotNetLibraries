namespace System.Globalization
{
    using System;
    using System.Text;

    internal class HebrewNumber
    {
        private static HebrewValue[] HebrewValues = new HebrewValue[] { 
            new HebrewValue(HebrewToken.Digit1, 1), new HebrewValue(HebrewToken.Digit1, 2), new HebrewValue(HebrewToken.Digit1, 3), new HebrewValue(HebrewToken.Digit1, 4), new HebrewValue(HebrewToken.Digit1, 5), new HebrewValue(HebrewToken.Digit6_7, 6), new HebrewValue(HebrewToken.Digit6_7, 7), new HebrewValue(HebrewToken.Digit1, 8), new HebrewValue(HebrewToken.Digit9, 9), new HebrewValue(HebrewToken.Digit10, 10), new HebrewValue(HebrewToken.Invalid, -1), new HebrewValue(HebrewToken.Digit10, 20), new HebrewValue(HebrewToken.Digit10, 30), new HebrewValue(HebrewToken.Invalid, -1), new HebrewValue(HebrewToken.Digit10, 40), new HebrewValue(HebrewToken.Invalid, -1), 
            new HebrewValue(HebrewToken.Digit10, 50), new HebrewValue(HebrewToken.Digit10, 60), new HebrewValue(HebrewToken.Digit10, 70), new HebrewValue(HebrewToken.Invalid, -1), new HebrewValue(HebrewToken.Digit10, 80), new HebrewValue(HebrewToken.Invalid, -1), new HebrewValue(HebrewToken.Digit10, 90), new HebrewValue(HebrewToken.Digit100, 100), new HebrewValue(HebrewToken.Digit200_300, 200), new HebrewValue(HebrewToken.Digit200_300, 300), new HebrewValue(HebrewToken.Digit400, 400)
         };
        private static char maxHebrewNumberCh = ((char) ((0x5d0 + HebrewValues.Length) - 1));
        private const int minHebrewNumberCh = 0x5d0;
        private static readonly HS[][] NumberPasingState = new HS[][] { 
            new HS[] { HS.S400, HS.X00, HS.X00, HS.X0, HS.X, HS.X, HS.X, HS.S9, HS._err, HS._err }, new HS[] { HS.S400_400, HS.S400_X00, HS.S400_X00, HS.S400_X0, HS._err, HS._err, HS._err, HS.X00_S9, HS.END, HS.S400_DQ }, new HS[] { HS._err, HS._err, HS.S400_400_100, HS.S400_X0, HS._err, HS._err, HS._err, HS.X00_S9, HS._err, HS.S400_400_DQ }, new HS[] { HS._err, HS._err, HS._err, HS.S400_X00_X0, HS._err, HS._err, HS._err, HS.X00_S9, HS._err, HS.X00_DQ }, new HS[] { HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS.X0_DQ }, new HS[] { HS._err, HS._err, HS._err, HS.END, HS.END, HS.END, HS.END, HS.END, HS._err, HS._err }, new HS[] { HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS.X0_DQ }, new HS[] { HS._err, HS._err, HS._err, HS._err, HS.END, HS.END, HS.END, HS.END, HS._err, HS._err }, new HS[] { HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS.END, HS._err }, new HS[] { HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS.END, HS.X0_DQ }, new HS[] { HS._err, HS._err, HS._err, HS.S400_X0, HS._err, HS._err, HS._err, HS.X00_S9, HS.END, HS.X00_DQ }, new HS[] { HS.END, HS.END, HS.END, HS.END, HS.END, HS.END, HS.END, HS.END, HS._err, HS._err }, new HS[] { HS._err, HS._err, HS.END, HS.END, HS.END, HS.END, HS.END, HS.END, HS._err, HS._err }, new HS[] { HS._err, HS._err, HS._err, HS.S400_X00_X0, HS._err, HS._err, HS._err, HS.X00_S9, HS._err, HS.X00_DQ }, new HS[] { HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS.END, HS.S9_DQ }, new HS[] { HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS._err, HS.S9_DQ }, 
            new HS[] { HS._err, HS._err, HS._err, HS._err, HS._err, HS.END, HS.END, HS._err, HS._err, HS._err }
         };

        private HebrewNumber()
        {
        }

        internal static bool IsDigit(char ch)
        {
            if ((ch >= 'א') && (ch <= maxHebrewNumberCh))
            {
                return (HebrewValues[ch - 'א'].value >= 0);
            }
            if (ch != '\'')
            {
                return (ch == '"');
            }
            return true;
        }

        internal static HebrewNumberParsingState ParseByChar(char ch, ref HebrewNumberParsingContext context)
        {
            HebrewToken singleQuote;
            if (ch == '\'')
            {
                singleQuote = HebrewToken.SingleQuote;
            }
            else if (ch == '"')
            {
                singleQuote = HebrewToken.DoubleQuote;
            }
            else
            {
                int index = ch - 'א';
                if ((index < 0) || (index >= HebrewValues.Length))
                {
                    return HebrewNumberParsingState.NotHebrewDigit;
                }
                singleQuote = HebrewValues[index].token;
                if (singleQuote == HebrewToken.Invalid)
                {
                    return HebrewNumberParsingState.NotHebrewDigit;
                }
                context.result += HebrewValues[index].value;
            }
            context.state = NumberPasingState[(int) context.state][(int) singleQuote];
            if (context.state == HS._err)
            {
                return HebrewNumberParsingState.InvalidHebrewNumber;
            }
            if (context.state == HS.END)
            {
                return HebrewNumberParsingState.FoundEndOfHebrewNumber;
            }
            return HebrewNumberParsingState.ContinueParsing;
        }

        internal static string ToString(int Number)
        {
            char ch = '\0';
            StringBuilder builder = new StringBuilder();
            if (Number > 0x1388)
            {
                Number -= 0x1388;
            }
            int num = Number / 100;
            if (num > 0)
            {
                Number -= num * 100;
                for (int i = 0; i < (num / 4); i++)
                {
                    builder.Append('ת');
                }
                int num4 = num % 4;
                if (num4 > 0)
                {
                    builder.Append((char) (0x5e6 + num4));
                }
            }
            int num2 = Number / 10;
            Number = Number % 10;
            switch (num2)
            {
                case 0:
                    ch = '\0';
                    break;

                case 1:
                    ch = 'י';
                    break;

                case 2:
                    ch = 'כ';
                    break;

                case 3:
                    ch = 'ל';
                    break;

                case 4:
                    ch = 'מ';
                    break;

                case 5:
                    ch = 'נ';
                    break;

                case 6:
                    ch = 'ס';
                    break;

                case 7:
                    ch = 'ע';
                    break;

                case 8:
                    ch = 'פ';
                    break;

                case 9:
                    ch = 'צ';
                    break;
            }
            char ch2 = (Number > 0) ? ((char) ((0x5d0 + Number) - 1)) : '\0';
            if ((ch2 == 'ה') && (ch == 'י'))
            {
                ch2 = 'ו';
                ch = 'ט';
            }
            if ((ch2 == 'ו') && (ch == 'י'))
            {
                ch2 = 'ז';
                ch = 'ט';
            }
            if (ch != '\0')
            {
                builder.Append(ch);
            }
            if (ch2 != '\0')
            {
                builder.Append(ch2);
            }
            if (builder.Length > 1)
            {
                builder.Insert(builder.Length - 1, '"');
            }
            else
            {
                builder.Append('\'');
            }
            return builder.ToString();
        }

        private enum HebrewToken
        {
            Digit1 = 4,
            Digit10 = 3,
            Digit100 = 2,
            Digit200_300 = 1,
            Digit400 = 0,
            Digit6_7 = 5,
            Digit7 = 6,
            Digit9 = 7,
            DoubleQuote = 9,
            Invalid = -1,
            SingleQuote = 8
        }

        private class HebrewValue
        {
            internal HebrewNumber.HebrewToken token;
            internal int value;

            internal HebrewValue(HebrewNumber.HebrewToken token, int value)
            {
                this.token = token;
                this.value = value;
            }
        }

        internal enum HS
        {
            _err = -1,
            END = 100,
            S400 = 1,
            S400_400 = 2,
            S400_400_100 = 13,
            S400_400_DQ = 12,
            S400_DQ = 11,
            S400_X0 = 4,
            S400_X00 = 3,
            S400_X00_X0 = 6,
            S9 = 14,
            S9_DQ = 0x10,
            Start = 0,
            X = 8,
            X0 = 9,
            X0_DQ = 7,
            X00 = 10,
            X00_DQ = 5,
            X00_S9 = 15
        }
    }
}

