namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class MaskedTextProvider : ICloneable
    {
        private static int ALLOW_PROMPT_AS_INPUT = BitVector32.CreateMask(ASCII_ONLY);
        private static int ASCII_ONLY = BitVector32.CreateMask();
        private int assignedCharCount;
        private const bool backward = false;
        private CultureInfo culture;
        private const bool defaultAllowPrompt = true;
        private const char defaultPromptChar = '_';
        private const byte editAny = 0;
        private const byte editAssigned = 2;
        private const byte editUnassigned = 1;
        private BitVector32 flagState;
        private const bool forward = true;
        private static int INCLUDE_LITERALS = BitVector32.CreateMask(INCLUDE_PROMPT);
        private static int INCLUDE_PROMPT = BitVector32.CreateMask(ALLOW_PROMPT_AS_INPUT);
        private const int invalidIndex = -1;
        private string mask;
        private static Type maskTextProviderType = typeof(MaskedTextProvider);
        private const char nullPasswordChar = '\0';
        private int optionalEditChars;
        private char passwordChar;
        private char promptChar;
        private int requiredCharCount;
        private int requiredEditChars;
        private static int RESET_ON_LITERALS = BitVector32.CreateMask(RESET_ON_PROMPT);
        private static int RESET_ON_PROMPT = BitVector32.CreateMask(INCLUDE_LITERALS);
        private static int SKIP_SPACE = BitVector32.CreateMask(RESET_ON_LITERALS);
        private const char spaceChar = ' ';
        private List<CharDescriptor> stringDescriptor;
        private StringBuilder testString;

        public MaskedTextProvider(string mask) : this(mask, null, true, '_', '\0', false)
        {
        }

        public MaskedTextProvider(string mask, bool restrictToAscii) : this(mask, null, true, '_', '\0', restrictToAscii)
        {
        }

        public MaskedTextProvider(string mask, CultureInfo culture) : this(mask, culture, true, '_', '\0', false)
        {
        }

        public MaskedTextProvider(string mask, char passwordChar, bool allowPromptAsInput) : this(mask, null, allowPromptAsInput, '_', passwordChar, false)
        {
        }

        public MaskedTextProvider(string mask, CultureInfo culture, bool restrictToAscii) : this(mask, culture, true, '_', '\0', restrictToAscii)
        {
        }

        public MaskedTextProvider(string mask, CultureInfo culture, char passwordChar, bool allowPromptAsInput) : this(mask, culture, allowPromptAsInput, '_', passwordChar, false)
        {
        }

        public MaskedTextProvider(string mask, CultureInfo culture, bool allowPromptAsInput, char promptChar, char passwordChar, bool restrictToAscii)
        {
            if (string.IsNullOrEmpty(mask))
            {
                throw new ArgumentException(SR.GetString("MaskedTextProviderMaskNullOrEmpty"), "mask");
            }
            foreach (char ch in mask)
            {
                if (!IsPrintableChar(ch))
                {
                    throw new ArgumentException(SR.GetString("MaskedTextProviderMaskInvalidChar"));
                }
            }
            if (culture == null)
            {
                culture = CultureInfo.CurrentCulture;
            }
            this.flagState = new BitVector32();
            this.mask = mask;
            this.promptChar = promptChar;
            this.passwordChar = passwordChar;
            if (!culture.IsNeutralCulture)
            {
                this.culture = culture;
            }
            else
            {
                foreach (CultureInfo info in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
                {
                    if (culture.Equals(info.Parent))
                    {
                        this.culture = info;
                        break;
                    }
                }
                if (this.culture == null)
                {
                    this.culture = CultureInfo.InvariantCulture;
                }
            }
            if (!this.culture.IsReadOnly)
            {
                this.culture = CultureInfo.ReadOnly(this.culture);
            }
            this.flagState[ALLOW_PROMPT_AS_INPUT] = allowPromptAsInput;
            this.flagState[ASCII_ONLY] = restrictToAscii;
            this.flagState[INCLUDE_PROMPT] = false;
            this.flagState[INCLUDE_LITERALS] = true;
            this.flagState[RESET_ON_PROMPT] = true;
            this.flagState[SKIP_SPACE] = true;
            this.flagState[RESET_ON_LITERALS] = true;
            this.Initialize();
        }

        public bool Add(char input)
        {
            int num;
            MaskedTextResultHint hint;
            return this.Add(input, out num, out hint);
        }

        public bool Add(string input)
        {
            int num;
            MaskedTextResultHint hint;
            return this.Add(input, out num, out hint);
        }

        public bool Add(char input, out int testPosition, out MaskedTextResultHint resultHint)
        {
            int lastAssignedPosition = this.LastAssignedPosition;
            if (lastAssignedPosition == (this.testString.Length - 1))
            {
                testPosition = this.testString.Length;
                resultHint = MaskedTextResultHint.UnavailableEditPosition;
                return false;
            }
            testPosition = lastAssignedPosition + 1;
            testPosition = this.FindEditPositionFrom(testPosition, true);
            if (testPosition == -1)
            {
                resultHint = MaskedTextResultHint.UnavailableEditPosition;
                testPosition = this.testString.Length;
                return false;
            }
            if (!this.TestSetChar(input, testPosition, out resultHint))
            {
                return false;
            }
            return true;
        }

        public bool Add(string input, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            testPosition = this.LastAssignedPosition + 1;
            if (input.Length == 0)
            {
                resultHint = MaskedTextResultHint.NoEffect;
                return true;
            }
            return this.TestSetString(input, testPosition, out testPosition, out resultHint);
        }

        public void Clear()
        {
            MaskedTextResultHint hint;
            this.Clear(out hint);
        }

        public void Clear(out MaskedTextResultHint resultHint)
        {
            if (this.assignedCharCount == 0)
            {
                resultHint = MaskedTextResultHint.NoEffect;
            }
            else
            {
                resultHint = MaskedTextResultHint.Success;
                for (int i = 0; i < this.testString.Length; i++)
                {
                    this.ResetChar(i);
                }
            }
        }

        public object Clone()
        {
            MaskedTextProvider provider;
            Type type = base.GetType();
            if (type == maskTextProviderType)
            {
                provider = new MaskedTextProvider(this.Mask, this.Culture, this.AllowPromptAsInput, this.PromptChar, this.PasswordChar, this.AsciiOnly);
            }
            else
            {
                object[] args = new object[] { this.Mask, this.Culture, this.AllowPromptAsInput, this.PromptChar, this.PasswordChar, this.AsciiOnly };
                provider = SecurityUtils.SecureCreateInstance(type, args) as MaskedTextProvider;
            }
            provider.ResetOnPrompt = false;
            provider.ResetOnSpace = false;
            provider.SkipLiterals = false;
            for (int i = 0; i < this.testString.Length; i++)
            {
                CharDescriptor charDescriptor = this.stringDescriptor[i];
                if (IsEditPosition(charDescriptor) && charDescriptor.IsAssigned)
                {
                    provider.Replace(this.testString[i], i);
                }
            }
            provider.ResetOnPrompt = this.ResetOnPrompt;
            provider.ResetOnSpace = this.ResetOnSpace;
            provider.SkipLiterals = this.SkipLiterals;
            provider.IncludeLiterals = this.IncludeLiterals;
            provider.IncludePrompt = this.IncludePrompt;
            return provider;
        }

        public int FindAssignedEditPositionFrom(int position, bool direction)
        {
            int num;
            int num2;
            if (this.assignedCharCount == 0)
            {
                return -1;
            }
            if (direction)
            {
                num = position;
                num2 = this.testString.Length - 1;
            }
            else
            {
                num = 0;
                num2 = position;
            }
            return this.FindAssignedEditPositionInRange(num, num2, direction);
        }

        public int FindAssignedEditPositionInRange(int startPosition, int endPosition, bool direction)
        {
            if (this.assignedCharCount == 0)
            {
                return -1;
            }
            return this.FindEditPositionInRange(startPosition, endPosition, direction, 2);
        }

        public int FindEditPositionFrom(int position, bool direction)
        {
            int num;
            int num2;
            if (direction)
            {
                num = position;
                num2 = this.testString.Length - 1;
            }
            else
            {
                num = 0;
                num2 = position;
            }
            return this.FindEditPositionInRange(num, num2, direction);
        }

        public int FindEditPositionInRange(int startPosition, int endPosition, bool direction)
        {
            CharType charTypeFlags = CharType.EditRequired | CharType.EditOptional;
            return this.FindPositionInRange(startPosition, endPosition, direction, charTypeFlags);
        }

        private int FindEditPositionInRange(int startPosition, int endPosition, bool direction, byte assignedStatus)
        {
            do
            {
                int num = this.FindEditPositionInRange(startPosition, endPosition, direction);
                if (num == -1)
                {
                    break;
                }
                CharDescriptor descriptor = this.stringDescriptor[num];
                switch (assignedStatus)
                {
                    case 1:
                        if (descriptor.IsAssigned)
                        {
                            break;
                        }
                        return num;

                    case 2:
                        if (!descriptor.IsAssigned)
                        {
                            break;
                        }
                        return num;

                    default:
                        return num;
                }
                if (direction)
                {
                    startPosition++;
                }
                else
                {
                    endPosition--;
                }
            }
            while (startPosition <= endPosition);
            return -1;
        }

        public int FindNonEditPositionFrom(int position, bool direction)
        {
            int num;
            int num2;
            if (direction)
            {
                num = position;
                num2 = this.testString.Length - 1;
            }
            else
            {
                num = 0;
                num2 = position;
            }
            return this.FindNonEditPositionInRange(num, num2, direction);
        }

        public int FindNonEditPositionInRange(int startPosition, int endPosition, bool direction)
        {
            CharType charTypeFlags = CharType.Literal | CharType.Separator;
            return this.FindPositionInRange(startPosition, endPosition, direction, charTypeFlags);
        }

        private int FindPositionInRange(int startPosition, int endPosition, bool direction, CharType charTypeFlags)
        {
            if (startPosition < 0)
            {
                startPosition = 0;
            }
            if (endPosition >= this.testString.Length)
            {
                endPosition = this.testString.Length - 1;
            }
            if (startPosition <= endPosition)
            {
                while (startPosition <= endPosition)
                {
                    int num = direction ? startPosition++ : endPosition--;
                    CharDescriptor descriptor = this.stringDescriptor[num];
                    if ((descriptor.CharType & charTypeFlags) == descriptor.CharType)
                    {
                        return num;
                    }
                }
                return -1;
            }
            return -1;
        }

        public int FindUnassignedEditPositionFrom(int position, bool direction)
        {
            int num;
            int num2;
            if (direction)
            {
                num = position;
                num2 = this.testString.Length - 1;
            }
            else
            {
                num = 0;
                num2 = position;
            }
            return this.FindEditPositionInRange(num, num2, direction, 1);
        }

        public int FindUnassignedEditPositionInRange(int startPosition, int endPosition, bool direction)
        {
            while (true)
            {
                int num = this.FindEditPositionInRange(startPosition, endPosition, direction, 0);
                if (num == -1)
                {
                    return -1;
                }
                CharDescriptor descriptor = this.stringDescriptor[num];
                if (!descriptor.IsAssigned)
                {
                    return num;
                }
                if (direction)
                {
                    startPosition++;
                }
                else
                {
                    endPosition--;
                }
            }
        }

        public static bool GetOperationResultFromHint(MaskedTextResultHint hint)
        {
            return (hint > MaskedTextResultHint.Unknown);
        }

        private void Initialize()
        {
            this.testString = new StringBuilder();
            this.stringDescriptor = new List<CharDescriptor>();
            CaseConversion none = CaseConversion.None;
            bool flag = false;
            int num = 0;
            CharType literal = CharType.Literal;
            string currencySymbol = string.Empty;
            for (int i = 0; i < this.mask.Length; i++)
            {
                CharDescriptor descriptor;
                char promptChar = this.mask[i];
                if (flag)
                {
                    goto Label_01C0;
                }
                char ch3 = promptChar;
                if (ch3 <= 'C')
                {
                    switch (ch3)
                    {
                        case '#':
                        case '9':
                        case '?':
                        case 'C':
                            goto Label_01A2;

                        case '$':
                            currencySymbol = this.culture.NumberFormat.CurrencySymbol;
                            literal = CharType.Separator;
                            goto Label_01C2;

                        case '&':
                        case '0':
                        case 'A':
                            goto Label_0188;

                        case ',':
                            currencySymbol = this.culture.NumberFormat.NumberGroupSeparator;
                            literal = CharType.Separator;
                            goto Label_01C2;

                        case '.':
                            currencySymbol = this.culture.NumberFormat.NumberDecimalSeparator;
                            literal = CharType.Separator;
                            goto Label_01C2;

                        case '/':
                            currencySymbol = this.culture.DateTimeFormat.DateSeparator;
                            literal = CharType.Separator;
                            goto Label_01C2;

                        case ':':
                            currencySymbol = this.culture.DateTimeFormat.TimeSeparator;
                            literal = CharType.Separator;
                            goto Label_01C2;

                        case '<':
                        {
                            none = CaseConversion.ToLower;
                            continue;
                        }
                        case '>':
                        {
                            none = CaseConversion.ToUpper;
                            continue;
                        }
                    }
                    goto Label_01BC;
                }
                if (ch3 <= '\\')
                {
                    switch (ch3)
                    {
                        case 'L':
                            goto Label_0188;

                        case '\\':
                            goto Label_017F;
                    }
                    goto Label_01BC;
                }
                if (ch3 == 'a')
                {
                    goto Label_01A2;
                }
                if (ch3 != '|')
                {
                    goto Label_01BC;
                }
                none = CaseConversion.None;
                continue;
            Label_017F:
                flag = true;
                literal = CharType.Literal;
                continue;
            Label_0188:
                this.requiredEditChars++;
                promptChar = this.promptChar;
                literal = CharType.EditRequired;
                goto Label_01C2;
            Label_01A2:
                this.optionalEditChars++;
                promptChar = this.promptChar;
                literal = CharType.EditOptional;
                goto Label_01C2;
            Label_01BC:
                literal = CharType.Literal;
                goto Label_01C2;
            Label_01C0:
                flag = false;
            Label_01C2:
                descriptor = new CharDescriptor(i, literal);
                if (IsEditPosition(descriptor))
                {
                    descriptor.CaseConversion = none;
                }
                if (literal != CharType.Separator)
                {
                    currencySymbol = promptChar.ToString();
                }
                foreach (char ch2 in currencySymbol)
                {
                    this.testString.Append(ch2);
                    this.stringDescriptor.Add(descriptor);
                    num++;
                }
            }
            this.testString.Capacity = this.testString.Length;
        }

        public bool InsertAt(char input, int position)
        {
            return (((position >= 0) && (position < this.testString.Length)) && this.InsertAt(input.ToString(), position));
        }

        public bool InsertAt(string input, int position)
        {
            int num;
            MaskedTextResultHint hint;
            return this.InsertAt(input, position, out num, out hint);
        }

        public bool InsertAt(char input, int position, out int testPosition, out MaskedTextResultHint resultHint)
        {
            return this.InsertAt(input.ToString(), position, out testPosition, out resultHint);
        }

        public bool InsertAt(string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if ((position >= 0) && (position < this.testString.Length))
            {
                return this.InsertAtInt(input, position, out testPosition, out resultHint, false);
            }
            testPosition = position;
            resultHint = MaskedTextResultHint.PositionOutOfRange;
            return false;
        }

        private bool InsertAtInt(string input, int position, out int testPosition, out MaskedTextResultHint resultHint, bool testOnly)
        {
            if (input.Length == 0)
            {
                testPosition = position;
                resultHint = MaskedTextResultHint.NoEffect;
                return true;
            }
            if (!this.TestString(input, position, out testPosition, out resultHint))
            {
                return false;
            }
            int startPosition = this.FindEditPositionFrom(position, true);
            bool flag = this.FindAssignedEditPositionInRange(startPosition, testPosition, true) != -1;
            int lastAssignedPosition = this.LastAssignedPosition;
            if (flag && (testPosition == (this.testString.Length - 1)))
            {
                resultHint = MaskedTextResultHint.UnavailableEditPosition;
                testPosition = this.testString.Length;
                return false;
            }
            int num3 = this.FindEditPositionFrom(testPosition + 1, true);
            if (!flag)
            {
                goto Label_00F3;
            }
            MaskedTextResultHint unknown = MaskedTextResultHint.Unknown;
        Label_007B:
            if (num3 == -1)
            {
                resultHint = MaskedTextResultHint.UnavailableEditPosition;
                testPosition = this.testString.Length;
                return false;
            }
            CharDescriptor descriptor = this.stringDescriptor[startPosition];
            if (descriptor.IsAssigned && !this.TestChar(this.testString[startPosition], num3, out unknown))
            {
                resultHint = unknown;
                testPosition = num3;
                return false;
            }
            if (startPosition != lastAssignedPosition)
            {
                startPosition = this.FindEditPositionFrom(startPosition + 1, true);
                num3 = this.FindEditPositionFrom(num3 + 1, true);
                goto Label_007B;
            }
            if (unknown > resultHint)
            {
                resultHint = unknown;
            }
        Label_00F3:
            if (!testOnly)
            {
                if (flag)
                {
                    while (startPosition >= position)
                    {
                        CharDescriptor descriptor2 = this.stringDescriptor[startPosition];
                        if (descriptor2.IsAssigned)
                        {
                            this.SetChar(this.testString[startPosition], num3);
                        }
                        else
                        {
                            this.ResetChar(num3);
                        }
                        num3 = this.FindEditPositionFrom(num3 - 1, false);
                        startPosition = this.FindEditPositionFrom(startPosition - 1, false);
                    }
                }
                this.SetString(input, position);
            }
            return true;
        }

        private static bool IsAciiAlphanumeric(char c)
        {
            if (((c < '0') || (c > '9')) && ((c < 'A') || (c > 'Z')))
            {
                return ((c >= 'a') && (c <= 'z'));
            }
            return true;
        }

        private static bool IsAlphanumeric(char c)
        {
            if (!char.IsLetter(c))
            {
                return char.IsDigit(c);
            }
            return true;
        }

        private static bool IsAscii(char c)
        {
            return ((c >= '!') && (c <= '~'));
        }

        private static bool IsAsciiLetter(char c)
        {
            return (((c >= 'A') && (c <= 'Z')) || ((c >= 'a') && (c <= 'z')));
        }

        public bool IsAvailablePosition(int position)
        {
            if ((position < 0) || (position >= this.testString.Length))
            {
                return false;
            }
            CharDescriptor charDescriptor = this.stringDescriptor[position];
            return (IsEditPosition(charDescriptor) && !charDescriptor.IsAssigned);
        }

        private static bool IsEditPosition(CharDescriptor charDescriptor)
        {
            if (charDescriptor.CharType != CharType.EditRequired)
            {
                return (charDescriptor.CharType == CharType.EditOptional);
            }
            return true;
        }

        public bool IsEditPosition(int position)
        {
            if ((position < 0) || (position >= this.testString.Length))
            {
                return false;
            }
            CharDescriptor charDescriptor = this.stringDescriptor[position];
            return IsEditPosition(charDescriptor);
        }

        private static bool IsLiteralPosition(CharDescriptor charDescriptor)
        {
            if (charDescriptor.CharType != CharType.Literal)
            {
                return (charDescriptor.CharType == CharType.Separator);
            }
            return true;
        }

        private static bool IsPrintableChar(char c)
        {
            if ((!char.IsLetterOrDigit(c) && !char.IsPunctuation(c)) && !char.IsSymbol(c))
            {
                return (c == ' ');
            }
            return true;
        }

        public static bool IsValidInputChar(char c)
        {
            return IsPrintableChar(c);
        }

        public static bool IsValidMaskChar(char c)
        {
            return IsPrintableChar(c);
        }

        public static bool IsValidPasswordChar(char c)
        {
            if (!IsPrintableChar(c))
            {
                return (c == '\0');
            }
            return true;
        }

        public bool Remove()
        {
            int num;
            MaskedTextResultHint hint;
            return this.Remove(out num, out hint);
        }

        public bool Remove(out int testPosition, out MaskedTextResultHint resultHint)
        {
            int lastAssignedPosition = this.LastAssignedPosition;
            if (lastAssignedPosition == -1)
            {
                testPosition = 0;
                resultHint = MaskedTextResultHint.NoEffect;
                return true;
            }
            this.ResetChar(lastAssignedPosition);
            testPosition = lastAssignedPosition;
            resultHint = MaskedTextResultHint.Success;
            return true;
        }

        public bool RemoveAt(int position)
        {
            return this.RemoveAt(position, position);
        }

        public bool RemoveAt(int startPosition, int endPosition)
        {
            int num;
            MaskedTextResultHint hint;
            return this.RemoveAt(startPosition, endPosition, out num, out hint);
        }

        public bool RemoveAt(int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (endPosition >= this.testString.Length)
            {
                testPosition = endPosition;
                resultHint = MaskedTextResultHint.PositionOutOfRange;
                return false;
            }
            if ((startPosition >= 0) && (startPosition <= endPosition))
            {
                return this.RemoveAtInt(startPosition, endPosition, out testPosition, out resultHint, false);
            }
            testPosition = startPosition;
            resultHint = MaskedTextResultHint.PositionOutOfRange;
            return false;
        }

        private bool RemoveAtInt(int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint, bool testOnly)
        {
            MaskedTextResultHint hint;
            char ch;
            char ch2;
            int lastAssignedPosition = this.LastAssignedPosition;
            int position = this.FindEditPositionInRange(startPosition, endPosition, true);
            resultHint = MaskedTextResultHint.NoEffect;
            if ((position == -1) || (position > lastAssignedPosition))
            {
                testPosition = startPosition;
                return true;
            }
            testPosition = startPosition;
            bool flag = endPosition < lastAssignedPosition;
            if (this.FindAssignedEditPositionInRange(startPosition, endPosition, true) != -1)
            {
                resultHint = MaskedTextResultHint.Success;
            }
            if (!flag)
            {
                goto Label_0131;
            }
            int num3 = this.FindEditPositionFrom(endPosition + 1, true);
            int num4 = num3;
            startPosition = position;
        Label_0051:
            ch = this.testString[num3];
            CharDescriptor descriptor = this.stringDescriptor[num3];
            if (((ch != this.PromptChar) || descriptor.IsAssigned) && !this.TestChar(ch, position, out hint))
            {
                resultHint = hint;
                testPosition = position;
                return false;
            }
            if (num3 != lastAssignedPosition)
            {
                num3 = this.FindEditPositionFrom(num3 + 1, true);
                position = this.FindEditPositionFrom(position + 1, true);
                goto Label_0051;
            }
            if (MaskedTextResultHint.SideEffect > resultHint)
            {
                resultHint = MaskedTextResultHint.SideEffect;
            }
            if (testOnly)
            {
                return true;
            }
            num3 = num4;
            position = startPosition;
        Label_00C8:
            ch2 = this.testString[num3];
            CharDescriptor descriptor2 = this.stringDescriptor[num3];
            if ((ch2 == this.PromptChar) && !descriptor2.IsAssigned)
            {
                this.ResetChar(position);
            }
            else
            {
                this.SetChar(ch2, position);
                this.ResetChar(num3);
            }
            if (num3 != lastAssignedPosition)
            {
                num3 = this.FindEditPositionFrom(num3 + 1, true);
                position = this.FindEditPositionFrom(position + 1, true);
                goto Label_00C8;
            }
            startPosition = position + 1;
        Label_0131:
            if (startPosition <= endPosition)
            {
                this.ResetString(startPosition, endPosition);
            }
            return true;
        }

        public bool Replace(char input, int position)
        {
            int num;
            MaskedTextResultHint hint;
            return this.Replace(input, position, out num, out hint);
        }

        public bool Replace(string input, int position)
        {
            int num;
            MaskedTextResultHint hint;
            return this.Replace(input, position, out num, out hint);
        }

        public bool Replace(char input, int position, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if ((position < 0) || (position >= this.testString.Length))
            {
                testPosition = position;
                resultHint = MaskedTextResultHint.PositionOutOfRange;
                return false;
            }
            testPosition = position;
            if (!this.TestEscapeChar(input, testPosition))
            {
                testPosition = this.FindEditPositionFrom(testPosition, true);
            }
            if (testPosition == -1)
            {
                resultHint = MaskedTextResultHint.UnavailableEditPosition;
                testPosition = position;
                return false;
            }
            if (!this.TestSetChar(input, testPosition, out resultHint))
            {
                return false;
            }
            return true;
        }

        public bool Replace(string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if ((position < 0) || (position >= this.testString.Length))
            {
                testPosition = position;
                resultHint = MaskedTextResultHint.PositionOutOfRange;
                return false;
            }
            if (input.Length == 0)
            {
                return this.RemoveAt(position, position, out testPosition, out resultHint);
            }
            if (!this.TestSetString(input, position, out testPosition, out resultHint))
            {
                return false;
            }
            return true;
        }

        public bool Replace(char input, int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (endPosition >= this.testString.Length)
            {
                testPosition = endPosition;
                resultHint = MaskedTextResultHint.PositionOutOfRange;
                return false;
            }
            if ((startPosition < 0) || (startPosition > endPosition))
            {
                testPosition = startPosition;
                resultHint = MaskedTextResultHint.PositionOutOfRange;
                return false;
            }
            if (startPosition == endPosition)
            {
                testPosition = startPosition;
                return this.TestSetChar(input, startPosition, out resultHint);
            }
            return this.Replace(input.ToString(), startPosition, endPosition, out testPosition, out resultHint);
        }

        public bool Replace(string input, int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint)
        {
            MaskedTextResultHint hint;
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (endPosition >= this.testString.Length)
            {
                testPosition = endPosition;
                resultHint = MaskedTextResultHint.PositionOutOfRange;
                return false;
            }
            if ((startPosition < 0) || (startPosition > endPosition))
            {
                testPosition = startPosition;
                resultHint = MaskedTextResultHint.PositionOutOfRange;
                return false;
            }
            if (input.Length == 0)
            {
                return this.RemoveAt(startPosition, endPosition, out testPosition, out resultHint);
            }
            if (!this.TestString(input, startPosition, out testPosition, out resultHint))
            {
                return false;
            }
            if (this.assignedCharCount <= 0)
            {
                goto Label_0162;
            }
            if (testPosition < endPosition)
            {
                int num;
                if (!this.RemoveAtInt(testPosition + 1, endPosition, out num, out hint, false))
                {
                    testPosition = num;
                    resultHint = hint;
                    return false;
                }
                if ((hint == MaskedTextResultHint.Success) && (resultHint != hint))
                {
                    resultHint = MaskedTextResultHint.SideEffect;
                }
                goto Label_0162;
            }
            if (testPosition <= endPosition)
            {
                goto Label_0162;
            }
            int lastAssignedPosition = this.LastAssignedPosition;
            int position = testPosition + 1;
            int num4 = endPosition + 1;
            while (true)
            {
                num4 = this.FindEditPositionFrom(num4, true);
                position = this.FindEditPositionFrom(position, true);
                if (position == -1)
                {
                    testPosition = this.testString.Length;
                    resultHint = MaskedTextResultHint.UnavailableEditPosition;
                    return false;
                }
                if (!this.TestChar(this.testString[num4], position, out hint))
                {
                    testPosition = position;
                    resultHint = hint;
                    return false;
                }
                if ((hint == MaskedTextResultHint.Success) && (resultHint != hint))
                {
                    resultHint = MaskedTextResultHint.Success;
                }
                if (num4 == lastAssignedPosition)
                {
                    goto Label_015C;
                }
                num4++;
                position++;
            }
        Label_0130:
            this.SetChar(this.testString[num4], position);
            num4 = this.FindEditPositionFrom(num4 - 1, false);
            position = this.FindEditPositionFrom(position - 1, false);
        Label_015C:
            if (position > testPosition)
            {
                goto Label_0130;
            }
        Label_0162:
            this.SetString(input, startPosition);
            return true;
        }

        private void ResetChar(int testPosition)
        {
            CharDescriptor descriptor = this.stringDescriptor[testPosition];
            if (this.IsEditPosition(testPosition) && descriptor.IsAssigned)
            {
                descriptor.IsAssigned = false;
                this.testString[testPosition] = this.promptChar;
                this.assignedCharCount--;
                if (descriptor.CharType == CharType.EditRequired)
                {
                    this.requiredCharCount--;
                }
            }
        }

        private void ResetString(int startPosition, int endPosition)
        {
            startPosition = this.FindAssignedEditPositionFrom(startPosition, true);
            if (startPosition != -1)
            {
                endPosition = this.FindAssignedEditPositionFrom(endPosition, false);
                while (startPosition <= endPosition)
                {
                    startPosition = this.FindAssignedEditPositionFrom(startPosition, true);
                    this.ResetChar(startPosition);
                    startPosition++;
                }
            }
        }

        public bool Set(string input)
        {
            int num;
            MaskedTextResultHint hint;
            return this.Set(input, out num, out hint);
        }

        public bool Set(string input, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            resultHint = MaskedTextResultHint.Unknown;
            testPosition = 0;
            if (input.Length == 0)
            {
                this.Clear(out resultHint);
                return true;
            }
            if (!this.TestSetString(input, testPosition, out testPosition, out resultHint))
            {
                return false;
            }
            int startPosition = this.FindAssignedEditPositionFrom(testPosition + 1, true);
            if (startPosition != -1)
            {
                this.ResetString(startPosition, this.testString.Length - 1);
            }
            return true;
        }

        private void SetChar(char input, int position)
        {
            CharDescriptor charDescriptor = this.stringDescriptor[position];
            this.SetChar(input, position, charDescriptor);
        }

        private void SetChar(char input, int position, CharDescriptor charDescriptor)
        {
            CharDescriptor local1 = this.stringDescriptor[position];
            if (this.TestEscapeChar(input, position, charDescriptor))
            {
                this.ResetChar(position);
            }
            else
            {
                if (char.IsLetter(input))
                {
                    if (char.IsUpper(input))
                    {
                        if (charDescriptor.CaseConversion == CaseConversion.ToLower)
                        {
                            input = this.culture.TextInfo.ToLower(input);
                        }
                    }
                    else if (charDescriptor.CaseConversion == CaseConversion.ToUpper)
                    {
                        input = this.culture.TextInfo.ToUpper(input);
                    }
                }
                this.testString[position] = input;
                if (!charDescriptor.IsAssigned)
                {
                    charDescriptor.IsAssigned = true;
                    this.assignedCharCount++;
                    if (charDescriptor.CharType == CharType.EditRequired)
                    {
                        this.requiredCharCount++;
                    }
                }
            }
        }

        private void SetString(string input, int testPosition)
        {
            foreach (char ch in input)
            {
                if (!this.TestEscapeChar(ch, testPosition))
                {
                    testPosition = this.FindEditPositionFrom(testPosition, true);
                }
                this.SetChar(ch, testPosition);
                testPosition++;
            }
        }

        private bool TestChar(char input, int position, out MaskedTextResultHint resultHint)
        {
            if (!IsPrintableChar(input))
            {
                resultHint = MaskedTextResultHint.InvalidInput;
                return false;
            }
            CharDescriptor charDescriptor = this.stringDescriptor[position];
            if (IsLiteralPosition(charDescriptor))
            {
                if (this.SkipLiterals && (input == this.testString[position]))
                {
                    resultHint = MaskedTextResultHint.CharacterEscaped;
                    return true;
                }
                resultHint = MaskedTextResultHint.NonEditPosition;
                return false;
            }
            if (input == this.promptChar)
            {
                if (this.ResetOnPrompt)
                {
                    if (IsEditPosition(charDescriptor) && charDescriptor.IsAssigned)
                    {
                        resultHint = MaskedTextResultHint.SideEffect;
                    }
                    else
                    {
                        resultHint = MaskedTextResultHint.CharacterEscaped;
                    }
                    return true;
                }
                if (!this.AllowPromptAsInput)
                {
                    resultHint = MaskedTextResultHint.PromptCharNotAllowed;
                    return false;
                }
            }
            if ((input == ' ') && this.ResetOnSpace)
            {
                if (IsEditPosition(charDescriptor) && charDescriptor.IsAssigned)
                {
                    resultHint = MaskedTextResultHint.SideEffect;
                }
                else
                {
                    resultHint = MaskedTextResultHint.CharacterEscaped;
                }
                return true;
            }
            switch (this.mask[charDescriptor.MaskPosition])
            {
                case '#':
                    if ((char.IsDigit(input) || (input == '-')) || ((input == '+') || (input == ' ')))
                    {
                        break;
                    }
                    resultHint = MaskedTextResultHint.DigitExpected;
                    return false;

                case '&':
                    if (IsAscii(input) || !this.AsciiOnly)
                    {
                        break;
                    }
                    resultHint = MaskedTextResultHint.AsciiCharacterExpected;
                    return false;

                case '0':
                    if (char.IsDigit(input))
                    {
                        break;
                    }
                    resultHint = MaskedTextResultHint.DigitExpected;
                    return false;

                case 'L':
                    if (!char.IsLetter(input))
                    {
                        resultHint = MaskedTextResultHint.LetterExpected;
                        return false;
                    }
                    if (IsAsciiLetter(input) || !this.AsciiOnly)
                    {
                        break;
                    }
                    resultHint = MaskedTextResultHint.AsciiCharacterExpected;
                    return false;

                case 'a':
                    if (!IsAlphanumeric(input) && (input != ' '))
                    {
                        resultHint = MaskedTextResultHint.AlphanumericCharacterExpected;
                        return false;
                    }
                    if (!IsAciiAlphanumeric(input) && this.AsciiOnly)
                    {
                        resultHint = MaskedTextResultHint.AsciiCharacterExpected;
                        return false;
                    }
                    break;

                case '?':
                    if (char.IsLetter(input) || (input == ' '))
                    {
                        if (IsAsciiLetter(input) || !this.AsciiOnly)
                        {
                            break;
                        }
                        resultHint = MaskedTextResultHint.AsciiCharacterExpected;
                        return false;
                    }
                    resultHint = MaskedTextResultHint.LetterExpected;
                    return false;

                case 'A':
                    if (IsAlphanumeric(input))
                    {
                        if (!IsAciiAlphanumeric(input) && this.AsciiOnly)
                        {
                            resultHint = MaskedTextResultHint.AsciiCharacterExpected;
                            return false;
                        }
                        break;
                    }
                    resultHint = MaskedTextResultHint.AlphanumericCharacterExpected;
                    return false;

                case 'C':
                    if ((IsAscii(input) || !this.AsciiOnly) || (input == ' '))
                    {
                        break;
                    }
                    resultHint = MaskedTextResultHint.AsciiCharacterExpected;
                    return false;

                case '9':
                    if (char.IsDigit(input) || (input == ' '))
                    {
                        break;
                    }
                    resultHint = MaskedTextResultHint.DigitExpected;
                    return false;
            }
            if ((input == this.testString[position]) && charDescriptor.IsAssigned)
            {
                resultHint = MaskedTextResultHint.NoEffect;
            }
            else
            {
                resultHint = MaskedTextResultHint.Success;
            }
            return true;
        }

        private bool TestEscapeChar(char input, int position)
        {
            CharDescriptor charDex = this.stringDescriptor[position];
            return this.TestEscapeChar(input, position, charDex);
        }

        private bool TestEscapeChar(char input, int position, CharDescriptor charDex)
        {
            if (IsLiteralPosition(charDex))
            {
                return (this.SkipLiterals && (input == this.testString[position]));
            }
            if ((!this.ResetOnPrompt || (input != this.promptChar)) && (!this.ResetOnSpace || (input != ' ')))
            {
                return false;
            }
            return true;
        }

        private bool TestSetChar(char input, int position, out MaskedTextResultHint resultHint)
        {
            if (!this.TestChar(input, position, out resultHint))
            {
                return false;
            }
            if ((resultHint == MaskedTextResultHint.Success) || (resultHint == MaskedTextResultHint.SideEffect))
            {
                this.SetChar(input, position);
            }
            return true;
        }

        private bool TestSetString(string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
        {
            if (this.TestString(input, position, out testPosition, out resultHint))
            {
                this.SetString(input, position);
                return true;
            }
            return false;
        }

        private bool TestString(string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
        {
            resultHint = MaskedTextResultHint.Unknown;
            testPosition = position;
            if (input.Length != 0)
            {
                MaskedTextResultHint hint = resultHint;
                foreach (char ch in input)
                {
                    if (testPosition >= this.testString.Length)
                    {
                        resultHint = MaskedTextResultHint.UnavailableEditPosition;
                        return false;
                    }
                    if (!this.TestEscapeChar(ch, testPosition))
                    {
                        testPosition = this.FindEditPositionFrom(testPosition, true);
                        if (testPosition == -1)
                        {
                            testPosition = this.testString.Length;
                            resultHint = MaskedTextResultHint.UnavailableEditPosition;
                            return false;
                        }
                    }
                    if (!this.TestChar(ch, testPosition, out hint))
                    {
                        resultHint = hint;
                        return false;
                    }
                    if (hint > resultHint)
                    {
                        resultHint = hint;
                    }
                    testPosition++;
                }
                testPosition--;
            }
            return true;
        }

        public string ToDisplayString()
        {
            if (!this.IsPassword || (this.assignedCharCount == 0))
            {
                return this.testString.ToString();
            }
            StringBuilder builder = new StringBuilder(this.testString.Length);
            for (int i = 0; i < this.testString.Length; i++)
            {
                CharDescriptor charDescriptor = this.stringDescriptor[i];
                builder.Append((IsEditPosition(charDescriptor) && charDescriptor.IsAssigned) ? this.passwordChar : this.testString[i]);
            }
            return builder.ToString();
        }

        public override string ToString()
        {
            return this.ToString(true, this.IncludePrompt, this.IncludeLiterals, 0, this.testString.Length);
        }

        public string ToString(bool ignorePasswordChar)
        {
            return this.ToString(ignorePasswordChar, this.IncludePrompt, this.IncludeLiterals, 0, this.testString.Length);
        }

        public string ToString(bool includePrompt, bool includeLiterals)
        {
            return this.ToString(true, includePrompt, includeLiterals, 0, this.testString.Length);
        }

        public string ToString(int startPosition, int length)
        {
            return this.ToString(true, this.IncludePrompt, this.IncludeLiterals, startPosition, length);
        }

        public string ToString(bool ignorePasswordChar, int startPosition, int length)
        {
            return this.ToString(ignorePasswordChar, this.IncludePrompt, this.IncludeLiterals, startPosition, length);
        }

        public string ToString(bool includePrompt, bool includeLiterals, int startPosition, int length)
        {
            return this.ToString(true, includePrompt, includeLiterals, startPosition, length);
        }

        public string ToString(bool ignorePasswordChar, bool includePrompt, bool includeLiterals, int startPosition, int length)
        {
            if (length <= 0)
            {
                return string.Empty;
            }
            if (startPosition < 0)
            {
                startPosition = 0;
            }
            if (startPosition >= this.testString.Length)
            {
                return string.Empty;
            }
            int num = this.testString.Length - startPosition;
            if (length > num)
            {
                length = num;
            }
            if ((!this.IsPassword || ignorePasswordChar) && (includePrompt && includeLiterals))
            {
                return this.testString.ToString(startPosition, length);
            }
            StringBuilder builder = new StringBuilder();
            int endPosition = (startPosition + length) - 1;
            if (!includePrompt)
            {
                int num3 = includeLiterals ? this.FindNonEditPositionInRange(startPosition, endPosition, false) : InvalidIndex;
                int num4 = this.FindAssignedEditPositionInRange((num3 == InvalidIndex) ? startPosition : num3, endPosition, false);
                endPosition = (num4 != InvalidIndex) ? num4 : num3;
                if (endPosition == InvalidIndex)
                {
                    return string.Empty;
                }
            }
            for (int i = startPosition; i <= endPosition; i++)
            {
                char ch = this.testString[i];
                CharDescriptor descriptor = this.stringDescriptor[i];
                switch (descriptor.CharType)
                {
                    case CharType.EditOptional:
                    case CharType.EditRequired:
                    {
                        if (!descriptor.IsAssigned)
                        {
                            break;
                        }
                        if (!this.IsPassword || ignorePasswordChar)
                        {
                            goto Label_013E;
                        }
                        builder.Append(this.passwordChar);
                        continue;
                    }
                    case CharType.Separator:
                    case CharType.Literal:
                        if (!includeLiterals)
                        {
                            continue;
                        }
                        goto Label_013E;

                    default:
                        goto Label_013E;
                }
                if (!includePrompt)
                {
                    builder.Append(' ');
                    continue;
                }
            Label_013E:
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public bool VerifyChar(char input, int position, out MaskedTextResultHint hint)
        {
            hint = MaskedTextResultHint.NoEffect;
            if ((position >= 0) && (position < this.testString.Length))
            {
                return this.TestChar(input, position, out hint);
            }
            hint = MaskedTextResultHint.PositionOutOfRange;
            return false;
        }

        public bool VerifyEscapeChar(char input, int position)
        {
            return (((position >= 0) && (position < this.testString.Length)) && this.TestEscapeChar(input, position));
        }

        public bool VerifyString(string input)
        {
            int num;
            MaskedTextResultHint hint;
            return this.VerifyString(input, out num, out hint);
        }

        public bool VerifyString(string input, out int testPosition, out MaskedTextResultHint resultHint)
        {
            testPosition = 0;
            if ((input != null) && (input.Length != 0))
            {
                return this.TestString(input, 0, out testPosition, out resultHint);
            }
            resultHint = MaskedTextResultHint.NoEffect;
            return true;
        }

        public bool AllowPromptAsInput
        {
            get
            {
                return this.flagState[ALLOW_PROMPT_AS_INPUT];
            }
        }

        public bool AsciiOnly
        {
            get
            {
                return this.flagState[ASCII_ONLY];
            }
        }

        public int AssignedEditPositionCount
        {
            get
            {
                return this.assignedCharCount;
            }
        }

        public int AvailableEditPositionCount
        {
            get
            {
                return (this.EditPositionCount - this.assignedCharCount);
            }
        }

        public CultureInfo Culture
        {
            get
            {
                return this.culture;
            }
        }

        public static char DefaultPasswordChar
        {
            get
            {
                return '*';
            }
        }

        public int EditPositionCount
        {
            get
            {
                return (this.optionalEditChars + this.requiredEditChars);
            }
        }

        public IEnumerator EditPositions
        {
            get
            {
                List<int> list = new List<int>();
                int item = 0;
                foreach (CharDescriptor descriptor in this.stringDescriptor)
                {
                    if (IsEditPosition(descriptor))
                    {
                        list.Add(item);
                    }
                    item++;
                }
                return list.GetEnumerator();
            }
        }

        public bool IncludeLiterals
        {
            get
            {
                return this.flagState[INCLUDE_LITERALS];
            }
            set
            {
                this.flagState[INCLUDE_LITERALS] = value;
            }
        }

        public bool IncludePrompt
        {
            get
            {
                return this.flagState[INCLUDE_PROMPT];
            }
            set
            {
                this.flagState[INCLUDE_PROMPT] = value;
            }
        }

        public static int InvalidIndex
        {
            get
            {
                return -1;
            }
        }

        public bool IsPassword
        {
            get
            {
                return (this.passwordChar != '\0');
            }
            set
            {
                if (this.IsPassword != value)
                {
                    this.passwordChar = value ? DefaultPasswordChar : '\0';
                }
            }
        }

        public char this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.testString.Length))
                {
                    throw new IndexOutOfRangeException(index.ToString(CultureInfo.CurrentCulture));
                }
                return this.testString[index];
            }
        }

        public int LastAssignedPosition
        {
            get
            {
                return this.FindAssignedEditPositionFrom(this.testString.Length - 1, false);
            }
        }

        public int Length
        {
            get
            {
                return this.testString.Length;
            }
        }

        public string Mask
        {
            get
            {
                return this.mask;
            }
        }

        public bool MaskCompleted
        {
            get
            {
                return (this.requiredCharCount == this.requiredEditChars);
            }
        }

        public bool MaskFull
        {
            get
            {
                return (this.assignedCharCount == this.EditPositionCount);
            }
        }

        public char PasswordChar
        {
            get
            {
                return this.passwordChar;
            }
            set
            {
                if (value == this.promptChar)
                {
                    throw new InvalidOperationException(SR.GetString("MaskedTextProviderPasswordAndPromptCharError"));
                }
                if (!IsValidPasswordChar(value) && (value != '\0'))
                {
                    throw new ArgumentException(SR.GetString("MaskedTextProviderInvalidCharError"));
                }
                if (value != this.passwordChar)
                {
                    this.passwordChar = value;
                }
            }
        }

        public char PromptChar
        {
            get
            {
                return this.promptChar;
            }
            set
            {
                if (value == this.passwordChar)
                {
                    throw new InvalidOperationException(SR.GetString("MaskedTextProviderPasswordAndPromptCharError"));
                }
                if (!IsPrintableChar(value))
                {
                    throw new ArgumentException(SR.GetString("MaskedTextProviderInvalidCharError"));
                }
                if (value != this.promptChar)
                {
                    this.promptChar = value;
                    for (int i = 0; i < this.testString.Length; i++)
                    {
                        CharDescriptor descriptor = this.stringDescriptor[i];
                        if (this.IsEditPosition(i) && !descriptor.IsAssigned)
                        {
                            this.testString[i] = this.promptChar;
                        }
                    }
                }
            }
        }

        public bool ResetOnPrompt
        {
            get
            {
                return this.flagState[RESET_ON_PROMPT];
            }
            set
            {
                this.flagState[RESET_ON_PROMPT] = value;
            }
        }

        public bool ResetOnSpace
        {
            get
            {
                return this.flagState[SKIP_SPACE];
            }
            set
            {
                this.flagState[SKIP_SPACE] = value;
            }
        }

        public bool SkipLiterals
        {
            get
            {
                return this.flagState[RESET_ON_LITERALS];
            }
            set
            {
                this.flagState[RESET_ON_LITERALS] = value;
            }
        }

        private enum CaseConversion
        {
            None,
            ToLower,
            ToUpper
        }

        private class CharDescriptor
        {
            public System.ComponentModel.MaskedTextProvider.CaseConversion CaseConversion;
            public System.ComponentModel.MaskedTextProvider.CharType CharType;
            public bool IsAssigned;
            public int MaskPosition;

            public CharDescriptor(int maskPos, System.ComponentModel.MaskedTextProvider.CharType charType)
            {
                this.MaskPosition = maskPos;
                this.CharType = charType;
            }

            public override string ToString()
            {
                return string.Format(CultureInfo.InvariantCulture, "MaskPosition[{0}] <CaseConversion.{1}><CharType.{2}><IsAssigned: {3}", new object[] { this.MaskPosition, this.CaseConversion, this.CharType, this.IsAssigned });
            }
        }

        [Flags]
        private enum CharType
        {
            EditOptional = 1,
            EditRequired = 2,
            Literal = 8,
            Modifier = 0x10,
            Separator = 4
        }
    }
}

